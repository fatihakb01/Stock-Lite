using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class PurchaseForecastConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly PurchaseForecastHelper _purchaseForecastHelper;

    public PurchaseForecastConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _purchaseForecastHelper = new PurchaseForecastHelper(fixture);
    }

    // -------------------------------------------------------
    // ForecastQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenForecastQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseForecastHelper.CreatePurchaseForecast(
                product,
                forecastQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenForecastQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var forecast = await _purchaseForecastHelper.CreatePurchaseForecast(
            product,
            forecastQuantity: 0);

        Assert.True(await Fixture.Context.PurchaseForecasts.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.WeekStartDate == forecast.WeekStartDate));
    }

    // -------------------------------------------------------
    // QuantityCommitted constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenQuantityCommittedIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseForecastHelper.CreatePurchaseForecast(
                product,
                quantityCommitted: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityCommittedIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var forecast = await _purchaseForecastHelper.CreatePurchaseForecast(
            product,
            quantityCommitted: 0);

        Assert.True(await Fixture.Context.PurchaseForecasts.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.WeekStartDate == forecast.WeekStartDate));
    }

    // -------------------------------------------------------
    // QuantityOnOrder constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenQuantityOnOrderIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseForecastHelper.CreatePurchaseForecast(
                product,
                quantityOnOrder: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityOnOrderIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var forecast = await _purchaseForecastHelper.CreatePurchaseForecast(
            product,
            quantityOnOrder: 0);

        Assert.True(await Fixture.Context.PurchaseForecasts.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.WeekStartDate == forecast.WeekStartDate));
    }

    // -------------------------------------------------------
    // RecommendedOrderQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenRecommendedOrderQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseForecastHelper.CreatePurchaseForecast(
                product,
                recommendedOrderQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenRecommendedOrderQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var forecast = await _purchaseForecastHelper.CreatePurchaseForecast(
            product,
            recommendedOrderQuantity: 0);

        Assert.True(await Fixture.Context.PurchaseForecasts.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.WeekStartDate == forecast.WeekStartDate));
    }

    // -------------------------------------------------------
    // Composite key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateProductAndWeekStartDate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var startDate = DateTime.UtcNow.Date;

        await _purchaseForecastHelper.CreatePurchaseForecast(
            product,
            weekStartDate: startDate);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _purchaseForecastHelper.CreatePurchaseForecast(
                product,
                weekStartDate: startDate));
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedPurchaseForecasts
    // inside ProductConfigurationTest.cs
}