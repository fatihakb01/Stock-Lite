using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class SalesAggregateConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly SalesAggregateHelper _salesAggregateHelper;

    public SalesAggregateConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _salesAggregateHelper = new SalesAggregateHelper(fixture);
    }

    // -------------------------------------------------------
    // TotalQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenTotalQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _salesAggregateHelper.CreateSalesAggregate(
                product,
                Period.Daily,
                totalQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenTotalQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var aggregate = await _salesAggregateHelper.CreateSalesAggregate(
            product,
            Period.Daily,
            totalQuantity: 0);

        Assert.True(await Fixture.Context.SalesAggregates.AnyAsync(x =>
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
            _salesAggregateHelper.CreateSalesAggregate(
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

        var aggregate = await _salesAggregateHelper.CreateSalesAggregate(
            product,
            Period.Daily,
            periodStartDate: date,
            periodEndDate: date);

        Assert.True(await Fixture.Context.SalesAggregates.AnyAsync(x =>
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

        await _salesAggregateHelper.CreateSalesAggregate(
            product,
            Period.Daily,
            periodStartDate: startDate);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _salesAggregateHelper.CreateSalesAggregate(
                product,
                Period.Daily,
                periodStartDate: startDate));
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedSalesAggregates
    // inside ProductConfigurationTest.cs
}