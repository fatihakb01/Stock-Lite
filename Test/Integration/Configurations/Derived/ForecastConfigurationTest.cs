using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class ForecastConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly ForecastHelper _forecastHelper;

    public ForecastConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _forecastHelper = new ForecastHelper(fixture);
    }

    // -------------------------------------------------------
    // Confidence constraint (> 0 and < 1)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenConfidenceIsZero_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _forecastHelper.CreateForecast(
                product,
                Period.Daily,
                confidence: 0));
    }

    [Fact]
    public async Task SaveChanges_WhenConfidenceIsOne_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _forecastHelper.CreateForecast(
                product,
                Period.Daily,
                confidence: 1));
    }

    [Fact]
    public async Task SaveChanges_WhenConfidenceIsBetweenZeroAndOne_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var forecast = await _forecastHelper.CreateForecast(
            product,
            Period.Daily,
            confidence: 0.5);

        Assert.True(await Fixture.Context.Forecasts.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.PeriodType == Period.Daily &&
            x.PeriodStartDate == forecast.PeriodStartDate));
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
            _forecastHelper.CreateForecast(
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

        var forecast = await _forecastHelper.CreateForecast(
            product,
            Period.Daily,
            periodStartDate: date,
            periodEndDate: date);

        Assert.True(await Fixture.Context.Forecasts.AnyAsync(x =>
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

        await _forecastHelper.CreateForecast(
            product,
            Period.Daily,
            periodStartDate: startDate);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _forecastHelper.CreateForecast(
                product,
                Period.Daily,
                periodStartDate: startDate));
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedForecasts
    // inside ProductConfigurationTest.cs
}