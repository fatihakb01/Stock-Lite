using Domain.Entities.Core;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Imports;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.Scheduler.Imports.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Scheduler.Imports.Handlers;

public class ProductImportHandlerTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly InventoryPolicyHelper _policyHelper = new InventoryPolicyHelper(fixture);
    private readonly Mock<IBaseCacheService<Product>> _cacheService = new Mock<IBaseCacheService<Product>>();
    private readonly Mock<ICompanyNotificationService> _companyNotificationService = new Mock<ICompanyNotificationService>();
    private readonly Mock<IProductNotificationService> _productNotificationService = new Mock<IProductNotificationService>();
    private readonly Mock<IImportHelper> _importHelper = new Mock<IImportHelper>();

    private ProductImportHandler CreateRepository()
        => new ProductImportHandler(_importHelper.Object, _cacheService.Object, _companyNotificationService.Object, _productNotificationService.Object);

    // -------------------------------------------------------
    // ImportAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsProduct()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv(
            sku: "SKU-0001",
            name: "Test Product",
            price: 25m,
            description: "Test Description",
            isActive: true);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var product = await _productHelper.GetProduct(
            company.Id,
            "SKU-0001");

        Assert.NotNull(product);
        Assert.Equal("SKU-0001", product.SKU);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(25m, product.Price);
        Assert.Equal("Test Description", product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsInventoryPolicy()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv(
            sku: "SKU-0001",
            leadTimeDays: 14,
            moq: 5,
            ioq: 10,
            safetyStock: 20,
            stockAdjustment: 3);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var policy = await _policyHelper.GetInventoryPolicy(
            company.Id,
            "SKU-0001");

        Assert.NotNull(policy);
        Assert.Equal(14, policy.LeadTimeDays);
        Assert.Equal(5, policy.MinimumOrderQuantity);
        Assert.Equal(10, policy.OrderIncrementQuantity);
        Assert.Equal(20, policy.SafetyStock);
        Assert.Equal(3, policy.StockAdjustment);
    }

    [Fact]
    public async Task ImportAsync_WhenProductAlreadyExists_UpdatesProduct()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            name: "Old Name",
            price: 10m);

        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv(
            sku: "SKU-0001",
            name: "Updated Name",
            price: 99m,
            description: "Updated Description",
            isActive: false,
            leadTimeDays: 30,
            moq: 15,
            ioq: 20,
            safetyStock: 50,
            stockAdjustment: 5);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var product = await _productHelper.GetProduct(
            company.Id,
            "SKU-0001");

        Assert.NotNull(product);
        Assert.Equal("Updated Name", product.Name);
        Assert.Equal(99m, product.Price);
        Assert.Equal("Updated Description", product.Description);
        Assert.False(product.IsActive);

        var policy = await _policyHelper.GetInventoryPolicy(
            company.Id,
            "SKU-0001");

        Assert.NotNull(policy);
        Assert.Equal(30, policy.LeadTimeDays);
        Assert.Equal(15, policy.MinimumOrderQuantity);
        Assert.Equal(20, policy.OrderIncrementQuantity);
        Assert.Equal(50, policy.SafetyStock);
        Assert.Equal(5, policy.StockAdjustment);
    }

    [Fact]
    public async Task ImportAsync_WhenDescriptionIsEmpty_ImportsSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv(
            sku: "SKU-0001",
            description: "");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var product = await _productHelper.GetProduct(
            company.Id,
            "SKU-0001");

        Assert.NotNull(product);
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public async Task ImportAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv();
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.ImportAsync(
                stream,
                company.Id,
                cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WhenCsvIsInvalid_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var invalidCsv =
            "InvalidHeader\n" +
            "InvalidValue";

        await using var stream = FileStreamHelper.CreateCsvStream(invalidCsv);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            handler.ImportAsync(
                stream,
                company.Id,
                CancellationToken.None));
    }

    [Fact]
    public async Task ImportAsync_CleansUpStagingTable()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ProductHelper.BuildCsv();
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var remainingRows = await Fixture.Context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM dbo.StagingProducts
                WHERE CompanyId = {0}
                """,
                company.Id)
            .SingleAsync();

        Assert.Equal(0, remainingRows);
    }
}