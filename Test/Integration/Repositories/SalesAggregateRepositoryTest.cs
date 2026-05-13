using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories;

public class SalesAggregateRepositoryTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly TransactionHelper _transactionHelper = new TransactionHelper(fixture);
    private readonly SalesAggregateHelper _salesHelper = new SalesAggregateHelper(fixture);
    private readonly DateTime _now = DateTime.UtcNow.Date;

    private SalesAggregateRepository CreateRepository() =>
        new(Fixture.Context);

    public static IEnumerable<object?[]> InsertPeriodData =>
    [
        [Period.Daily, 30],
        [Period.Weekly, 13],
        [Period.Monthly, 36],
        [null, 79]
    ];

    public static IEnumerable<object[]> UpdatePeriodData =>
    [
        [Period.Daily],
        [Period.Weekly],
        [Period.Monthly],
    ];

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetByProductIdAsync_WhenPeriodAndSales_ReturnsForecast(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _salesHelper.CreateSalesAggregate(product, period);

        var result = await repo.GetByProductIdAsync(product.Id, period);

        Assert.NotEmpty(result);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetByProductIdAsync_WhenPeriodAndNoSales_ReturnsForecast(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var result = await repo.GetByProductIdAsync(product.Id, period);

        Assert.Empty(result);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenPeriod_InsertsRows(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _salesHelper.GetSalesCountAsync(product.Id, period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenCalledTwice_DoesNotDuplicate(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _salesHelper.GetSalesCountAsync(product.Id, period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenCompanySpecified_OnlyInsertsForThatCompany(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = await _productHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

        await repo.InsertSalesAggregatesAsync(company1.Id, [product1.Id], period);
        int count1 = await _salesHelper.GetSalesCountAsync(product1.Id, period);
        int count2 = await _salesHelper.GetSalesCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenProductIdsNotSpecified_InsertsForAllProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        await repo.InsertSalesAggregatesAsync(company.Id, [], period);
        int count1 = await _salesHelper.GetSalesCountAsync(product1.Id, period);
        int count2 = await _salesHelper.GetSalesCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(rows, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        await repo.InsertSalesAggregatesAsync(company.Id, [product1.Id], period);
        int count1 = await _salesHelper.GetSalesCountAsync(product1.Id, period);
        int count2 = await _salesHelper.GetSalesCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenPeriod_UpdatesPeriodTotals(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        var transaction = await _transactionHelper.CreateTransaction(company, _now);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 5,
            unitPrice: 10);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        var aggregate = await _salesHelper.GetSalesAsync(product.Id, period);

        Assert.Equal(5, aggregate.TotalQuantity);
        Assert.Equal(50, aggregate.TotalRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(company: company, name: "Test Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(company: company, name: "Test Product 2", sku: "SKU-0002");

        var transaction = await _transactionHelper.CreateTransaction(company,_now);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product1,
            quantity: 5,
            unitPrice: 10);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product2,
            quantity: 20,
            unitPrice: 10);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product1.Id, product2.Id],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product1.Id],
            period);

        var aggregate1 = await _salesHelper.GetSalesAsync(product1.Id, period);
        var aggregate2 = await _salesHelper.GetSalesAsync(product2.Id, period);

        Assert.Equal(5, aggregate1.TotalQuantity);
        Assert.Equal(50, aggregate1.TotalRevenue);

        Assert.Equal(0, aggregate2.TotalQuantity);
        Assert.Equal(0, aggregate2.TotalRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenProductIdsNotSpecified_UpdatesAllProducts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product1 = await _productHelper.CreateProduct(company: company, name: "Test Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(company: company, name: "Test Product 2", sku: "SKU-0002");

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            _now);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product1,
            quantity: 5,
            unitPrice: 10);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product2,
            quantity: 20,
            unitPrice: 10);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [],
            period);

        var aggregate1 = await _salesHelper.GetSalesAsync(product1.Id, period);
        var aggregate2 = await _salesHelper.GetSalesAsync(product2.Id, period);

        Assert.Equal(5, aggregate1.TotalQuantity);
        Assert.Equal(50, aggregate1.TotalRevenue);

        Assert.Equal(20, aggregate2.TotalQuantity);
        Assert.Equal(200, aggregate2.TotalRevenue);
    }

    [Fact]
    public async Task UpdateSalesAggregatesAsync_WhenNoPeriod_UpdatesAllPeriods()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(company,_now);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 5,
            unitPrice: 10);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        var daily = await _salesHelper.GetSalesAsync(product.Id, Period.Daily);
        var weekly = await _salesHelper.GetSalesAsync(product.Id, Period.Weekly);
        var monthly = await _salesHelper.GetSalesAsync(product.Id, Period.Monthly);

        Assert.Equal(5, daily.TotalQuantity);
        Assert.Equal(50, daily.TotalRevenue);

        Assert.Equal(5, weekly.TotalQuantity);
        Assert.Equal(50, weekly.TotalRevenue);

        Assert.Equal(5, monthly.TotalQuantity);
        Assert.Equal(50, monthly.TotalRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenCompanySpecified_OnlyUpdatesThatCompany(Period period)
    {
        var repo = CreateRepository();

        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var product1 = await _productHelper.CreateProduct(
            company: company1,
            name: "Product 1",
            sku: "SKU-0001");

        var company2 = await _companyHelper.CreateCompany(name: "Company 2");
        var product2 = await _productHelper.CreateProduct(
            company: company2,
            name: "Product 2",
            sku: "SKU-0002");

        var transaction1 = await _transactionHelper.CreateTransaction(
            company1,
            _now);

        var transaction2 = await _transactionHelper.CreateTransaction(
            company2,
            _now);

        await _transactionHelper.CreateTransactionLineItem(
            transaction1,
            product1,
            quantity: 5,
            unitPrice: 10);

        await _transactionHelper.CreateTransactionLineItem(
            transaction2,
            product2,
            quantity: 20,
            unitPrice: 10);

        await repo.InsertSalesAggregatesAsync(
            company1.Id,
            [product1.Id],
            period);

        await repo.InsertSalesAggregatesAsync(
            company2.Id,
            [product2.Id],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company1.Id,
            [product1.Id],
            period);

        var aggregate1 = await _salesHelper.GetSalesAsync(product1.Id, period);
        var aggregate2 = await _salesHelper.GetSalesAsync(product2.Id, period);

        // Company 1 SHOULD be updated
        Assert.Equal(5, aggregate1.TotalQuantity);
        Assert.Equal(50, aggregate1.TotalRevenue);

        // Company 2 should remain untouched
        Assert.Equal(0, aggregate2.TotalQuantity);
        Assert.Equal(0, aggregate2.TotalRevenue);
    }
}