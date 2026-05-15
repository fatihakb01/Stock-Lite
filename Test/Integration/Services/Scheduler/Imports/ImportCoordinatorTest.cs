using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports;
using Infrastructure.Services.Scheduler.Imports.Handlers;
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
            new ProductImportHandler(Fixture.Context),
            new TransactionImportHandler(Fixture.Context),
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
            ImportDataType.Products =>
                "SKU;Name;Price;Description;IsActive;LeadTimeDays;MinimumOrderQuantity;OrderIncrementQuantity;SafetyStock;StockAdjustment\n" +
                $"{Sku};Product A;10.5;Desc;true;7;1;1;5;0",

            ImportDataType.UserDefinedFields =>
                "SKU;Key;Value\n" +
                $"{Sku};Stock;10",

            ImportDataType.ConfirmedOrders =>
                "OrderNumber;SKU;OrderDate;ExpectedDeliveryDate;ReceivedDate;Quantity;SoldQuantity\n" +
                $"CO-1;{Sku};2026-05-01;2026-05-05;2026-05-02;10;10",

            ImportDataType.PurchaseOrders =>
                "OrderNumber;SKU;OrderDate;ExpectedDeliveryDate;ReceivedDate;Quantity;ReceivedQuantity\n" +
                $"PO-1;{Sku};2026-05-01;2026-05-05;2026-05-02;10;10",

            ImportDataType.Transactions =>
                "TransactionId;SKU;TransactionDate;PaymentMethod;Notes;Quantity\n" +
                $"T-1;{Sku};2026-05-01;Cash;Note;10",

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