using Domain.Entities.Derived;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories;

public class SalesAggregateRepositoryTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
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
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenPeriod_InsertsRows(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = 0;
        if (period == null)
        {
            count = await Fixture.Context.SalesAggregates
                .CountAsync(x => x.ProductId == product.Id);
        }
        else
        {
            count = await Fixture.Context.SalesAggregates
                .CountAsync(x =>
                    x.ProductId == product.Id &&
                    x.PeriodType == period
                );
        }

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertSalesAggregatesAsync_WhenCalledTwice_DoesNotDuplicate(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        int count = 0;
        if (period == null)
        {
            count = await Fixture.Context.SalesAggregates
                .CountAsync(x => x.ProductId == product.Id);
        }
        else
        {
            count = await Fixture.Context.SalesAggregates
                .CountAsync(x =>
                    x.ProductId == product.Id &&
                    x.PeriodType == period
                );
        }

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenPeriod_UpdatesPeriodTotals(Period? period)
    {
        var repo = CreateRepository();
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);

        var transaction = TransactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date);

        await Fixture.Context.Transactions.AddAsync(transaction);

        var lineItem = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 5,
            unitPrice: 10);

        await Fixture.Context.TransactionLineItems.AddAsync(lineItem);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product.Id],
            period);

        var correctDate = DateHelper.GetPeriodStartDate(period);
        var aggregate = await Fixture.Context.SalesAggregates
            .SingleAsync(x =>
                x.ProductId == product.Id &&
                x.PeriodType == period &&
                x.PeriodStartDate == correctDate);

        if (period == null)
        {
            aggregate = await Fixture.Context.SalesAggregates
                .SingleAsync(x => x.ProductId == product.Id);
        }

        Assert.Equal(5, aggregate.TotalQuantity);
        Assert.Equal(50, aggregate.TotalRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateSalesAggregatesAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts(Period period)
    {
        var repo = CreateRepository();
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product1 = ProductHelper.CreateProduct(company: company, name: "Test Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company: company, name: "Test Product 2", sku: "SKU-0002");
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var transaction = TransactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date);
        await Fixture.Context.Transactions.AddAsync(transaction);

        var lineItem1 = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product1,
            quantity: 5,
            unitPrice: 10);

        var lineItem2 = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product2,
            quantity: 20,
            unitPrice: 10);

        await Fixture.Context.TransactionLineItems.AddRangeAsync(
            lineItem1,
            lineItem2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product1.Id, product2.Id],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product1.Id],
            period);

        var correctDate = DateHelper.GetPeriodStartDate(period);
        var aggregate1 = await GetAggregateAsync(product1.Id, period);
        var aggregate2 = await GetAggregateAsync(product2.Id, period);

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
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product1 = ProductHelper.CreateProduct(company: company, name: "Test Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company: company, name: "Test Product 2", sku: "SKU-0002");
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var transaction = TransactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date);
        await Fixture.Context.Transactions.AddAsync(transaction);

        var lineItem1 = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product1,
            quantity: 5,
            unitPrice: 10);

        var lineItem2 = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product2,
            quantity: 20,
            unitPrice: 10);

        await Fixture.Context.TransactionLineItems.AddRangeAsync(
            lineItem1,
            lineItem2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [],
            period);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [],
            period);

        var correctDate = DateHelper.GetPeriodStartDate(period);
        var aggregate1 = await GetAggregateAsync(product1.Id, period);
        var aggregate2 = await GetAggregateAsync(product2.Id, period);

        Assert.Equal(5, aggregate1.TotalQuantity);
        Assert.Equal(50, aggregate1.TotalRevenue);

        Assert.Equal(20, aggregate2.TotalQuantity);
        Assert.Equal(200, aggregate2.TotalRevenue);
    }

    [Fact]
    public async Task UpdateSalesAggregatesAsync_WhenNoPeriod_UpdatesAllPeriods()
    {
        var repo = CreateRepository();
        var company = CompanyHelper.CreateCompany();
        await Fixture.Context.Companies.AddAsync(company);

        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);

        var transaction = TransactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date);
        await Fixture.Context.Transactions.AddAsync(transaction);

        var lineItem = TransactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 5,
            unitPrice: 10);

        await Fixture.Context.TransactionLineItems.AddAsync(lineItem);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertSalesAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        await repo.UpdateSalesAggregatesAsync(
            company.Id,
            [product.Id],
            null);

        var daily = await GetAggregateAsync(product.Id, Period.Daily);
        var weekly = await GetAggregateAsync(product.Id, Period.Weekly);
        var monthly = await GetAggregateAsync(product.Id, Period.Monthly);

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

        var company1 = CompanyHelper.CreateCompany(name: "Company 1");
        var product1 = ProductHelper.CreateProduct(
            company: company1,
            name: "Product 1",
            sku: "SKU-0001");

        var company2 = CompanyHelper.CreateCompany(name: "Company 2");
        var product2 = ProductHelper.CreateProduct(
            company: company2,
            name: "Product 2",
            sku: "SKU-0002");

        await Fixture.Context.Companies.AddRangeAsync(company1, company2);
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var transaction1 = TransactionHelper.CreateTransaction(
            company1,
            DateTime.UtcNow.Date);

        var transaction2 = TransactionHelper.CreateTransaction(
            company2,
            DateTime.UtcNow.Date);

        await Fixture.Context.Transactions.AddRangeAsync(
            transaction1,
            transaction2);

        var lineItem1 = TransactionHelper.CreateTransactionLineItem(
            transaction1,
            product1,
            quantity: 5,
            unitPrice: 10);

        var lineItem2 = TransactionHelper.CreateTransactionLineItem(
            transaction2,
            product2,
            quantity: 20,
            unitPrice: 10);

        await Fixture.Context.TransactionLineItems.AddRangeAsync(
            lineItem1,
            lineItem2);

        await Fixture.Context.SaveChangesAsync();

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

        var aggregate1 = await GetAggregateAsync(product1.Id, period);
        var aggregate2 = await GetAggregateAsync(product2.Id, period);

        // Company 1 SHOULD be updated
        Assert.Equal(5, aggregate1.TotalQuantity);
        Assert.Equal(50, aggregate1.TotalRevenue);

        // Company 2 should remain untouched
        Assert.Equal(0, aggregate2.TotalQuantity);
        Assert.Equal(0, aggregate2.TotalRevenue);
    }

    private async Task<SalesAggregate> GetAggregateAsync(
        Guid productId,
        Period period)
    {
        var correctDate = DateHelper.GetPeriodStartDate(period);

        return await Fixture.Context.SalesAggregates
            .SingleAsync(x =>
                x.ProductId == productId &&
                x.PeriodType == period &&
                x.PeriodStartDate == correctDate);
    }
}