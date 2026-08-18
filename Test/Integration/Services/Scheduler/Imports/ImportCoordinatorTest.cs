using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Imports;
using Domain.Interfaces.Services.SignalR;
using Infrastructure.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports.Handlers;
using Moq;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Scheduler.Imports;

public class ImportCoordinatorTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly TransactionHelper _transactionHelper = new TransactionHelper(fixture);
    private readonly ConfirmedOrderHelper _coHelper = new ConfirmedOrderHelper(fixture);
    private readonly PurchaseOrderHelper _poHelper = new PurchaseOrderHelper(fixture);
    private readonly UserDefinedFieldHelper _udfHelper = new UserDefinedFieldHelper(fixture);
    private readonly Mock<IBaseCacheService<Product>> _productCacheService = new Mock<IBaseCacheService<Product>>();
    private readonly Mock<IBaseCacheService<Transaction>> _transactionCacheService = new Mock<IBaseCacheService<Transaction>>();
    private readonly Mock<ICompanyNotificationService> _companyNotificationService = new Mock<ICompanyNotificationService>();
    private readonly Mock<IProductNotificationService> _productNotificationService = new Mock<IProductNotificationService>();
    private readonly Mock<IImportHelper> _importHelper = new Mock<IImportHelper>();
    private readonly string Sku = "SKU-0001";

    private ImportCoordinator CreateCoordinator(IEnumerable<IImportHandler> handlers)
        => new ImportCoordinator(handlers);

    // -------------------------------------------------------
    // File helpers
    // -------------------------------------------------------
    private static string CreateCsvFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    private static void CleanupFile(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore cleanup issues in tests
        }
    }

    // -------------------------------------------------------
    // Handlers (real integration)
    // -------------------------------------------------------
    private IEnumerable<IImportHandler> CreateHandlers()
    {
        return new IImportHandler[]
        {
            new ProductImportHandler(_importHelper.Object, _productCacheService.Object, _companyNotificationService.Object, _productNotificationService.Object),
            new TransactionImportHandler(_importHelper.Object, _transactionCacheService.Object, _productNotificationService.Object),
            new ConfirmedOrderImportHandler(Fixture.Context),
            new PurchaseOrderImportHandler(Fixture.Context),
            new UserDefinedFieldImportHandler(Fixture.Context)
        };
    }

    // -------------------------------------------------------
    // CSV generation
    // -------------------------------------------------------
    private string BuildCsv(ImportDataType type)
    {
        return type switch
        {
            ImportDataType.Products => ProductHelper.BuildCsv(sku: Sku),

            ImportDataType.UserDefinedFields => UserDefinedFieldHelper.BuildCsv(sku: Sku),

            ImportDataType.ConfirmedOrders => ConfirmedOrderHelper.BuildCsv(sku: Sku),

            ImportDataType.PurchaseOrders => PurchaseOrderHelper.BuildCsv(sku: Sku),

            ImportDataType.Transactions => TransactionHelper.BuildCsv(sku: Sku),

            _ => throw new InvalidOperationException("Unsupported type")
        };
    }

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------
    [Theory]
    [InlineData(ImportDataType.Products)]
    [InlineData(ImportDataType.Transactions)]
    [InlineData(ImportDataType.ConfirmedOrders)]
    [InlineData(ImportDataType.PurchaseOrders)]
    [InlineData(ImportDataType.UserDefinedFields)]
    public async Task RunAsync_ShouldImportData_WhenValidCsvProvided(ImportDataType type)
    {
        var company = await _companyHelper.CreateCompany();

        var path = CreateCsvFile(BuildCsv(type));

        try
        {
            if (type != ImportDataType.Products)
                await _productHelper.CreateProduct(sku: Sku, company: company);

            var coordinator = CreateCoordinator(CreateHandlers());

            await coordinator.RunAsync(
                type,
                path,
                company.Id,
                CancellationToken.None);

            switch (type)
            {
                case ImportDataType.Products:
                    Assert.True(await _productHelper.CountProducts(company.Id) > 0);
                    break;

                case ImportDataType.Transactions:
                    Assert.True(await _transactionHelper.CountTransactions(company.Id) > 0);
                    break;

                case ImportDataType.ConfirmedOrders:
                    Assert.True(await _coHelper.CountConfirmedOrders(company.Id) > 0);
                    break;

                case ImportDataType.PurchaseOrders:
                    Assert.True(await _poHelper.CountPurchaseOrders(company.Id) > 0);
                    break;

                case ImportDataType.UserDefinedFields:
                    Assert.True(await _udfHelper.CountUserDefinedFields(company.Id) > 0);
                    break;
            }
        }
        finally
        {
            CleanupFile(path);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldThrow_WhenHandlerMissing()
    {
        var company = await _companyHelper.CreateCompany();
        var path = CreateCsvFile("dummy");

        try
        {
            var coordinator = CreateCoordinator(Array.Empty<IImportHandler>());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                coordinator.RunAsync(
                    (ImportDataType)999,
                    path,
                    company.Id,
                    CancellationToken.None));

            Assert.Contains("No import handler", ex.Message);
        }
        finally
        {
            CleanupFile(path);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldThrow_WhenFileDoesNotExist()
    {
        var company = await _companyHelper.CreateCompany();
        var coordinator = CreateCoordinator(CreateHandlers());

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            coordinator.RunAsync(
                ImportDataType.Products,
                "C:\\invalid\\file.csv",
                company.Id,
                CancellationToken.None));
    }

    [Fact]
    public async Task RunAsync_ShouldRespectCancellation()
    {
        var company = await _companyHelper.CreateCompany();
        var path = CreateCsvFile(BuildCsv(ImportDataType.Products));

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try
        {
            var coordinator = CreateCoordinator(CreateHandlers());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                coordinator.RunAsync(
                    ImportDataType.Products,
                    path,
                    company.Id,
                    cts.Token));
        }
        finally
        {
            CleanupFile(path);
        }
    }
}