using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class ConfirmedOrderAggregateConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly ConfirmedOrderAggregateHelper _aggregateHelper;

    public ConfirmedOrderAggregateConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _aggregateHelper = new ConfirmedOrderAggregateHelper(fixture);
    }

    // -------------------------------------------------------
    // TotalConfirmedQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenTotalConfirmedQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _aggregateHelper.CreateConfirmedOrderAggregate(
                product,
                Period.Daily,
                totalConfirmedQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenTotalConfirmedQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var aggregate = await _aggregateHelper.CreateConfirmedOrderAggregate(
            product,
            Period.Daily,
            totalConfirmedQuantity: 0);

        Assert.True(await Fixture.Context.ConfirmedOrderAggregates.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.PeriodType == Period.Daily &&
            x.PeriodStartDate == aggregate.PeriodStartDate));
    }

    // -------------------------------------------------------
    // Period date constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenPeriodStartDateIsAfterPeriodEndDate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _aggregateHelper.CreateConfirmedOrderAggregate(
                product,
                Period.Daily,
                periodStartDate: DateTime.UtcNow.Date,
                periodEndDate: DateTime.UtcNow.Date.AddDays(-1)));
    }

    [Fact]
    public async Task SaveChanges_WhenPeriodStartDateEqualsPeriodEndDate_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var date = DateTime.UtcNow.Date;

        var aggregate = await _aggregateHelper.CreateConfirmedOrderAggregate(
            product,
            Period.Daily,
            periodStartDate: date,
            periodEndDate: date);

        Assert.True(await Fixture.Context.ConfirmedOrderAggregates.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.PeriodType == Period.Daily &&
            x.PeriodStartDate == date));
    }

    // -------------------------------------------------------
    // Composite key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateProductPeriodAndStartDate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var startDate = DateHelper.GetPeriodStartDate(Period.Daily);

        await _aggregateHelper.CreateConfirmedOrderAggregate(
            product,
            Period.Daily,
            periodStartDate: startDate);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _aggregateHelper.CreateConfirmedOrderAggregate(
                product,
                Period.Daily,
                periodStartDate: startDate));
    }

    [Fact]
    public async Task SaveChanges_WhenSameProductDifferentPeriodStartDate_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var date1 = DateHelper.GetPeriodStartDate(Period.Daily);
        var date2 = date1.AddMonths(1);

        await _aggregateHelper.CreateConfirmedOrderAggregate(product, Period.Daily, periodStartDate: date1, periodEndDate: date1);
        await _aggregateHelper.CreateConfirmedOrderAggregate(product, Period.Daily, periodStartDate: date2, periodEndDate: date2);

        int count = await Fixture.Context.ConfirmedOrderAggregates
            .CountAsync(s => s.ProductId == product.Id);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task SaveChanges_WhenSamePeriodStartDateDifferentProduct_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product1 = await _productHelper.CreateProduct(
            company, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(
            company, name: "Product 2", sku: "SKU-0002");

        var periodStartDate = DateHelper.GetPeriodStartDate(Period.Daily);

        await _aggregateHelper.CreateConfirmedOrderAggregate(product1, Period.Daily, periodStartDate: periodStartDate);
        await _aggregateHelper.CreateConfirmedOrderAggregate(product2, Period.Daily, periodStartDate: periodStartDate);

        int count = await Fixture.Context.ConfirmedOrderAggregates
            .CountAsync(s =>
                s.PeriodStartDate == periodStartDate &&
                (s.ProductId == product1.Id || s.ProductId == product2.Id));

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedConfirmedOrderAggregates inside ProductConfigurationTest.cs
}