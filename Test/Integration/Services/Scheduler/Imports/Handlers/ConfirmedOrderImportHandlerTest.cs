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

public class ConfirmedOrderImportHandlerTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly ConfirmedOrderHelper _coHelper = new ConfirmedOrderHelper(fixture);
    private readonly Mock<IImportHelper> _importHelper = new Mock<IImportHelper>();
    private readonly Mock<IProductNotificationService> _productNotificationService = new Mock<IProductNotificationService>();
    private readonly Mock<IBaseCacheService<ConfirmedOrder>> _confirmedOrderCacheService = new Mock<IBaseCacheService<ConfirmedOrder>>();

    private ConfirmedOrderImportHandler CreateRepository()
        => new(_importHelper.Object, _confirmedOrderCacheService.Object, _productNotificationService.Object);

    // -------------------------------------------------------
    // ImportAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsConfirmedOrder()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _coHelper.GetConfirmedOrder(company.Id, "CO-0001");

        Assert.NotNull(order);
        Assert.Equal(company.Id, order.CompanyId);
        Assert.Equal("CO-0001", order.OrderNumber);
        Assert.Equal(new DateTime(2026, 05, 01), order.OrderDate);
        Assert.Equal(new DateTime(2026, 05, 05), order.ExpectedDeliveryDate);
        Assert.Equal(new DateTime(2026, 05, 02), order.ReceivedDate);
    }

    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsLineItem()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            quantity: 10,
            soldQuantity: 5,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var lineItem = await _coHelper.GetLineItem(
            company.Id,
            "CO-0001",
            "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(10, lineItem.Quantity);
        Assert.Equal(5, lineItem.SoldQuantity);
        Assert.Equal(15m, lineItem.UnitCost);
    }

    [Fact]
    public async Task ImportAsync_WhenOrderAlreadyExists_UpdatesOrder()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            quantity: 10, 
            soldQuantity: 2,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using (var initialStream = FileStreamHelper.CreateCsvStream(csv))
        {
            await handler.ImportAsync(
                initialStream,
                company.Id,
                CancellationToken.None);
        }

        csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            quantity: 20,
            soldQuantity: 10,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using var updateStream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            updateStream,
            company.Id,
            CancellationToken.None);

        var lineItem = await _coHelper.GetLineItem(
            company.Id,
            "CO-0001",
            "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(20, lineItem.Quantity);
        Assert.Equal(10, lineItem.SoldQuantity);
    }

    [Fact]
    public async Task ImportAsync_WhenProductDoesNotExist_DoesNotInsertLineItem()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _coHelper.GetConfirmedOrder(company.Id, "CO-0001");

        Assert.NotNull(order);

        var lineItems = await _coHelper.CountConfirmedOrders(company.Id);

        Assert.Equal(0, lineItems);
    }

    [Fact]
    public async Task ImportAsync_WhenReceivedDateIsNull_ImportsSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _coHelper.GetConfirmedOrder(company.Id, "CO-0001");

        Assert.NotNull(order);
        Assert.Null(order.ReceivedDate);
    }

    [Fact]
    public async Task ImportAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

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

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 15m);

        var handler = CreateRepository();

        var csv = ConfirmedOrderHelper.BuildCsv(
            orderNumber: "CO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var remainingRows = await Fixture.Context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM dbo.StagingConfirmedOrders
                WHERE CompanyId = {0}
                """,
                company.Id)
            .SingleAsync();

        Assert.Equal(0, remainingRows);
    }
}