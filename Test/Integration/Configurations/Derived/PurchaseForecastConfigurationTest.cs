using Domain.Entities.Core;
using Domain.Enums;
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

    [Fact]
    public async Task SaveChanges_WhenSameProductDifferentWeekStartDate_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var date1 = DateHelper.GetPeriodStartDate(Period.Weekly);
        var date2 = date1.AddDays(7);

        await _purchaseForecastHelper.CreatePurchaseForecast(product, weekStartDate: date1);
        await _purchaseForecastHelper.CreatePurchaseForecast(product, weekStartDate: date2);

        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(s => s.ProductId == product.Id);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task SaveChanges_WhenSameWeekStartDateDifferentProduct_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product1 = await _productHelper.CreateProduct(
            company, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(
            company, name: "Product 2", sku: "SKU-0002");

        var date = DateHelper.GetPeriodStartDate(Period.Weekly);

        await _purchaseForecastHelper.CreatePurchaseForecast(product1, weekStartDate: date);
        await _purchaseForecastHelper.CreatePurchaseForecast(product2, weekStartDate: date);

        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(s =>
                s.WeekStartDate == date &&
                (s.ProductId == product1.Id || s.ProductId == product2.Id));

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedPurchaseForecasts
    // inside ProductConfigurationTest.cs
}