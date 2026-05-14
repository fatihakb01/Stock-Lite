using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories;

public class ConfirmedOrderAggregateRepositoryTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly ConfirmedOrderAggregateHelper _coAggregateHelper = new ConfirmedOrderAggregateHelper(fixture);
    private readonly ConfirmedOrderHelper _confirmedOrderHelper = new ConfirmedOrderHelper(fixture);
    private readonly DateTime _now = DateTime.UtcNow.Date;

    private ConfirmedOrderAggregateRepository CreateRepository() =>
        new(Fixture.Context);

    public static IEnumerable<object?[]> InsertPeriodData =>
    [
        [Period.Daily, 30],
        [Period.Weekly, 13],
        [Period.Monthly, 12],
        [null, 55]
    ];

    public static IEnumerable<object[]> UpdatePeriodData =>
    [
        [Period.Daily],
        [Period.Weekly],
        [Period.Monthly]
    ];

    private async Task SeedConfirmedOrderAsync(
        Company company,
        Product product,
        string orderNumber = "CO-0001",
        int quantity = 10,
        decimal unitCost = 5,
        int soldQuantity = 0,
        DateTime? expectedDeliveryDate = null,
        ConfirmedOrderStatus status = ConfirmedOrderStatus.Confirmed)
    {
        var order = await _confirmedOrderHelper.CreateConfirmedOrder(
            company,
            orderNumber: orderNumber,
            expectedDeliveryDate: expectedDeliveryDate ?? _now,
            status: status);

        await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: quantity,
            soldQuantity: soldQuantity,
            unitCost: unitCost);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetByProductIdAsync_WhenAggregateExists_ReturnsAggregate(
        Period period)
    {
        var repo = CreateRepository();

        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _coAggregateHelper.CreateConfirmedOrderAggregate(product, period, 100, 1000);

        var result = await repo.GetByProductIdAsync(product.Id, period);

        Assert.NotEmpty(result);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetByProductIdAsync_WhenAggregateDoesNotExist_ReturnsEmpty(
        Period period)
    {
        var repo = CreateRepository();

        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var result = await repo.GetByProductIdAsync(product.Id, period);

        Assert.Empty(result);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertConfirmedOrderAggregatesAsync_WhenPeriod_InsertsRows(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();

        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product.Id,
            period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertConfirmedOrderAggregatesAsync_WhenCalledTwice_DoesNotDuplicate(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();

        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product.Id,
            period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertConfirmedOrderAggregatesAsync_WhenCompanySpecified_OnlyInsertsForThatCompany(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();

        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = await _productHelper.CreateProduct(
            company1,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company2,
            name: "Product 2",
            sku: "SKU-0002");

        await repo.InsertConfirmedOrderAggregatesAsync(
            company1.Id,
            [product1.Id],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertConfirmedOrderAggregatesAsync_WhenProductIdsNotSpecified_InsertsForAllProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(rows, count1);
        Assert.Equal(rows, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertConfirmedOrderAggregatesAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product1.Id],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteConfirmedOrderAggregatesAsync_DeletesPastAggregates(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await repo.DeleteConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product.Id,
            period);

        Assert.Equal(0, count);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteConfirmedOrderAggregatesAsync_DoesNotDeleteFutureAggregates(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product,
            period,
            100, 
            1000);

        await repo.DeleteConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product.Id,
            period);

        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteConfirmedOrderAggregatesAsync_WhenProductIdsSpecified_OnlyDeletesSpecifiedProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product1,
            period,
            100, 
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product2,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await repo.DeleteConfirmedOrderAggregatesAsync(
            company.Id,
            [product1.Id],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteConfirmedOrderAggregatesAsync_WhenProductIdsNotSpecified_DeletesAllProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product1,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product2,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await repo.DeleteConfirmedOrderAggregatesAsync(
            company.Id,
            [],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(0, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteConfirmedOrderAggregatesAsync_WhenCompanySpecified_OnlyDeletesThatCompany(
        Period period)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = await _productHelper.CreateProduct(
            company1,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company2,
            name: "Product 2",
            sku: "SKU-0002");

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product1,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await _coAggregateHelper.CreateConfirmedOrderAggregate(
            product2,
            period,
            100,
            1000,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await repo.DeleteConfirmedOrderAggregatesAsync(
            company1.Id,
            [product1.Id],
            period);

        int count1 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product1.Id,
            period);

        int count2 = await _coAggregateHelper.GetConfirmedOrderAggregatesCountAsync(
            product2.Id,
            period);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateConfirmedOrderAggregatesAsync_WhenPeriod_UpdatesAggregateValues(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await SeedConfirmedOrderAsync(
            company,
            product,
            quantity: 10,
            unitCost: 5,
            soldQuantity: 2);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.UpdateConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        var aggregate = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product.Id,
            period);

        Assert.True(aggregate.TotalConfirmedQuantity > 0);
        Assert.True(aggregate.TotalConfirmedRevenue > 0);
    }

    [Fact]
    public async Task UpdateConfirmedOrderAggregatesAsync_WhenNoPeriod_UpdatesAllPeriods()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await SeedConfirmedOrderAsync(
            company,
            product,
            quantity: 10,
            unitCost: 5,
            soldQuantity: 2);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        await repo.UpdateConfirmedOrderAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        var daily = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product.Id,
            Period.Daily);

        var weekly = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product.Id,
            Period.Weekly);

        var monthly = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product.Id,
            Period.Monthly);

        Assert.True(daily.TotalConfirmedQuantity > 0);
        Assert.True(daily.TotalConfirmedRevenue > 0);

        Assert.True(weekly.TotalConfirmedQuantity > 0);
        Assert.True(weekly.TotalConfirmedRevenue > 0);

        Assert.True(monthly.TotalConfirmedQuantity > 0);
        Assert.True(monthly.TotalConfirmedRevenue > 0);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateConfirmedOrderAggregatesAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await SeedConfirmedOrderAsync(company, product1, orderNumber: "CO-0001", quantity: 10);
        await SeedConfirmedOrderAsync(company, product2, orderNumber: "CO-0002", quantity: 20);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [product1.Id, product2.Id],
            null);

        await repo.UpdateConfirmedOrderAggregatesAsync(
            company.Id,
            [product1.Id],
            null);

        var aggregate1 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product1.Id,
            period);

        var aggregate2 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product2.Id,
            period);

        Assert.True(aggregate1.TotalConfirmedQuantity > 0);
        Assert.True(aggregate1.TotalConfirmedRevenue > 0);

        Assert.Equal(0, aggregate2.TotalConfirmedQuantity);
        Assert.Equal(0, aggregate2.TotalConfirmedRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateConfirmedOrderAggregatesAsync_WhenProductIdsNotSpecified_UpdatesAllProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = await _productHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        await SeedConfirmedOrderAsync(company, product1, orderNumber: "CO-0001", quantity: 10);
        await SeedConfirmedOrderAsync(company, product2, orderNumber: "CO-0002", quantity: 20);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company.Id,
            [],
            null);

        await repo.UpdateConfirmedOrderAggregatesAsync(
            company.Id,
            [],
            null);

        var aggregate1 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product1.Id,
            period);

        var aggregate2 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product2.Id,
            period);

        Assert.True(aggregate1.TotalConfirmedQuantity > 0);
        Assert.True(aggregate1.TotalConfirmedRevenue > 0);

        Assert.True(aggregate2.TotalConfirmedQuantity > 0);
        Assert.True(aggregate2.TotalConfirmedRevenue > 0);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateConfirmedOrderAggregatesAsync_WhenCompanySpecified_OnlyUpdatesThatCompany(
        Period period)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = await _productHelper.CreateProduct(
            company1,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company2,
            name: "Product 2",
            sku: "SKU-0002");

        await SeedConfirmedOrderAsync(company1, product1, orderNumber: "CO-0001", quantity: 10);
        await SeedConfirmedOrderAsync(company2, product2, orderNumber: "CO-0002", quantity: 20);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company1.Id,
            [product1.Id],
            null);

        await repo.InsertConfirmedOrderAggregatesAsync(
            company2.Id,
            [product2.Id],
            null);

        await repo.UpdateConfirmedOrderAggregatesAsync(
            company1.Id,
            [product1.Id],
            null);

        var aggregate1 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product1.Id,
            period);

        var aggregate2 = await _coAggregateHelper.GetConfirmedOrderAggregateAsync(
            product2.Id,
            period);

        Assert.True(aggregate1.TotalConfirmedQuantity > 0);
        Assert.True(aggregate1.TotalConfirmedRevenue > 0);

        Assert.Equal(0, aggregate2.TotalConfirmedQuantity);
        Assert.Equal(0, aggregate2.TotalConfirmedRevenue);
    }
}