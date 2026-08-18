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

public class TransactionImportHandlerTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly TransactionHelper _transactionHelper = new TransactionHelper(fixture);
    private readonly Mock<IBaseCacheService<Transaction>> _cacheService = new Mock<IBaseCacheService<Transaction>>();
    private readonly Mock<IProductNotificationService> _productNotificationService = new Mock<IProductNotificationService>();
    private readonly Mock<IImportHelper> _importHelper = new Mock<IImportHelper>();

    private TransactionImportHandler CreateRepository()
        => new TransactionImportHandler(_importHelper.Object, _cacheService.Object, _productNotificationService.Object);

    // -------------------------------------------------------
    // ImportAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsTransaction()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 12.5m);

        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001",
            transactionDate: "2026-05-01",
            paymentMethod: "Cash",
            notes: "Test transaction",
            quantity: 10);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var transaction = await _transactionHelper.GetTransaction(
            company.Id,
            "TR-0001");

        Assert.NotNull(transaction);
        Assert.Equal(company.Id, transaction.CompanyId);
        Assert.Equal("TR-0001", transaction.TransactionId);
        Assert.Equal(new DateTime(2026, 05, 01), transaction.TransactionDate);
        Assert.Equal("Cash", transaction.PaymentMethod);
        Assert.Equal("Test transaction", transaction.Notes);
        Assert.Equal(125m, transaction.TotalAmount); // 10 * 12.5
    }

    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsLineItem()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 20m);

        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001",
            quantity: 5);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var lineItem = await _transactionHelper.GetLineItem(
            company.Id,
            "TR-0001",
            "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(5, lineItem.Quantity);
        Assert.Equal(20m, lineItem.UnitPrice);
    }

    [Fact]
    public async Task ImportAsync_WhenTransactionAlreadyExists_UpdatesTransaction()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 10m);

        var handler = CreateRepository();

        var initialCsv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001",
            paymentMethod: "Cash",
            notes: "Initial",
            quantity: 2);

        await using (var initialStream = FileStreamHelper.CreateCsvStream(initialCsv))
        {
            await handler.ImportAsync(
                initialStream,
                company.Id,
                CancellationToken.None);
        }

        var updatedCsv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001",
            paymentMethod: "Card",
            notes: "Updated",
            quantity: 10);

        await using var updateStream = FileStreamHelper.CreateCsvStream(updatedCsv);

        await handler.ImportAsync(
            updateStream,
            company.Id,
            CancellationToken.None);

        var transaction = await _transactionHelper.GetTransaction(
            company.Id,
            "TR-0001");

        Assert.NotNull(transaction);
        Assert.Equal("Card", transaction.PaymentMethod);
        Assert.Equal("Updated", transaction.Notes);
        Assert.Equal(100m, transaction.TotalAmount); // 10 * 10

        var lineItem = await _transactionHelper.GetLineItem(
            company.Id,
            "TR-0001",
            "SKU-0001");

        Assert.NotNull(lineItem);
        Assert.Equal(10, lineItem.Quantity);
    }

    [Fact]
    public async Task ImportAsync_WhenProductDoesNotExist_DoesNotInsertLineItem()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "UNKNOWN-SKU",
            quantity: 5);

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var lineItemCount = await _transactionHelper.CountTransactions(company.Id);

        Assert.Equal(0, lineItemCount);
    }

    [Fact]
    public async Task ImportAsync_WhenPaymentMethodAndNotesAreEmpty_ImportsSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 10m);

        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001",
            paymentMethod: "",
            notes: "");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var transaction = await _transactionHelper.GetTransaction(
            company.Id,
            "TR-0001");

        Assert.NotNull(transaction);
        Assert.Equal(string.Empty, transaction.PaymentMethod);
        Assert.Equal(string.Empty, transaction.Notes);
    }

    [Fact]
    public async Task ImportAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001",
            price: 10m);

        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001");

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
            price: 10m);

        var handler = CreateRepository();

        var csv = TransactionHelper.BuildCsv(
            transactionId: "TR-0001",
            sku: "SKU-0001");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(
            stream,
            company.Id,
            CancellationToken.None);

        var remainingRows = await Fixture.Context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM dbo.StagingTransactions
                WHERE CompanyId = {0}
                """,
                company.Id)
            .SingleAsync();

        Assert.Equal(0, remainingRows);
    }
}