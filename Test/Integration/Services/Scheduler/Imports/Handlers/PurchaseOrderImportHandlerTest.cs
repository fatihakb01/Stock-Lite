using Infrastructure.Services.Scheduler.Imports.Handlers;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Scheduler.Imports.Handlers;

public class PurchaseOrderImportHandlerTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly PurchaseOrderHelper _poHelper = new PurchaseOrderHelper(fixture);

    private PurchaseOrderImportHandler CreateRepository()
        => new PurchaseOrderImportHandler(Fixture.Context);

    // -------------------------------------------------------
    // Helper
    // -------------------------------------------------------
    private static MemoryStream CreateCsvStream(string csv)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(csv));
    }

    // -------------------------------------------------------
    // ImportAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsPurchaseOrder()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 25m);

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using var stream = CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _poHelper.GetPurchaseOrder(company.Id, "PO-0001");

        Assert.NotNull(order);
        Assert.Equal(company.Id, order.CompanyId);
        Assert.Equal("PO-0001", order.OrderNumber);
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
            price: 25m);

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001",
            quantity: 20,
            receivedQuantity: 5,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using var stream = CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var lineItem = await _poHelper.GetLineItem(company.Id, "PO-0001", "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(20, lineItem.Quantity);
        Assert.Equal(5, lineItem.ReceivedQuantity);
        Assert.Equal(25m, lineItem.UnitCost);
    }

    [Fact]
    public async Task ImportAsync_WhenOrderAlreadyExists_UpdatesOrder()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 25m);

        var handler = CreateRepository();

        var initialCsv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001",
            quantity: 10,
            receivedQuantity: 2,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "2026-05-02");

        await using (var initialStream = CreateCsvStream(initialCsv))
        {
            await handler.ImportAsync(
                initialStream,
                company.Id,
                CancellationToken.None);
        }

        var updatedCsv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001",
            quantity: 50,
            receivedQuantity: 40,
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-10",
            receivedDate: "2026-05-08");

        await using var updateStream = CreateCsvStream(updatedCsv);

        await handler.ImportAsync(
            updateStream,
            company.Id,
            CancellationToken.None);

        var lineItem = await _poHelper.GetLineItem(company.Id, "PO-0001", "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(50, lineItem.Quantity);
        Assert.Equal(40, lineItem.ReceivedQuantity);

        var order = await _poHelper.GetPurchaseOrder(company.Id, "PO-0001");

        Assert.NotNull(order);
        Assert.Equal(new DateTime(2026, 05, 10), order.ExpectedDeliveryDate);
        Assert.Equal(new DateTime(2026, 05, 08), order.ReceivedDate);
    }

    [Fact]
    public async Task ImportAsync_WhenProductDoesNotExist_DoesNotInsertLineItem()
    {
        var company = await _companyHelper.CreateCompany();

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "UNKNOWN-SKU",
            quantity: 10,
            receivedQuantity: 5);

        await using var stream = CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _poHelper.GetPurchaseOrder(company.Id, "PO-0001");

        Assert.NotNull(order);

        var lineItemCount = await _poHelper.CountPurchaseOrders(company.Id);

        Assert.Equal(0, lineItemCount);
    }

    [Fact]
    public async Task ImportAsync_WhenReceivedDateIsNull_ImportsSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 25m);

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001",
            orderDate: "2026-05-01",
            expectedDeliveryDate: "2026-05-05",
            receivedDate: "");

        await using var stream = CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var order = await _poHelper.GetPurchaseOrder(company.Id, "PO-0001");

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
            price: 25m);

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001");

        await using var stream = CreateCsvStream(csv);

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

        await using var stream = CreateCsvStream(invalidCsv);

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
            price: 25m);

        var handler = CreateRepository();

        var csv = PurchaseOrderHelper.BuildCsv(
            orderNumber: "PO-0001",
            sku: "SKU-0001");

        await using var stream = CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var remainingRows = await Fixture.Context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM dbo.StagingPurchaseOrders
                WHERE CompanyId = {0}
                """,
                company.Id)
            .SingleAsync();

        Assert.Equal(0, remainingRows);
    }
}