using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class SeasonalityConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly SeasonalityHelper _seasonalityHelper;

    public SeasonalityConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _seasonalityHelper = new SeasonalityHelper(fixture);
    }

    // -------------------------------------------------------
    // Factor constraint (>= 0 AND <= 12)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenFactorIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _seasonalityHelper.CreateSeasonality(product, factor: -0.01m));
    }

    [Fact]
    public async Task SaveChanges_WhenFactorIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var seasonality = await _seasonalityHelper.CreateSeasonality(product, factor: 0m);

        Assert.True(await Fixture.Context.Seasonality.AnyAsync(s =>
            s.ProductId == product.Id &&
            s.PeriodStartDate == seasonality.PeriodStartDate));
    }

    [Fact]
    public async Task SaveChanges_WhenFactorIsOne_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var seasonality = await _seasonalityHelper.CreateSeasonality(product, factor: 1m);

        Assert.True(await Fixture.Context.Seasonality.AnyAsync(s =>
            s.ProductId == product.Id &&
            s.PeriodStartDate == seasonality.PeriodStartDate));
    }

    [Fact]
    public async Task SaveChanges_WhenFactorIsAtMaximum_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var seasonality = await _seasonalityHelper.CreateSeasonality(product, factor: 12m);

        Assert.True(await Fixture.Context.Seasonality.AnyAsync(s =>
            s.ProductId == product.Id &&
            s.PeriodStartDate == seasonality.PeriodStartDate));
    }

    [Fact]
    public async Task SaveChanges_WhenFactorExceedsMaximum_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _seasonalityHelper.CreateSeasonality(product, factor: 12.01m));
    }

    // -------------------------------------------------------
    // Composite key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateProductAndPeriodStartDate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var periodStartDate = DateTime.UtcNow.Date;

        await _seasonalityHelper.CreateSeasonality(
            product, factor: 1m, periodStartDate: periodStartDate);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _seasonalityHelper.CreateSeasonality(
                product, factor: 1.5m, periodStartDate: periodStartDate));
    }

    [Fact]
    public async Task SaveChanges_WhenSameProductDifferentPeriodStartDate_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var date1 = DateHelper.GetPeriodStartDate(Period.Daily);
        var date2 = date1.AddMonths(1);

        await _seasonalityHelper.CreateSeasonality(product, factor: 1m, periodStartDate: date1);
        await _seasonalityHelper.CreateSeasonality(product, factor: 1.2m, periodStartDate: date2);

        int count = await Fixture.Context.Seasonality
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

        await _seasonalityHelper.CreateSeasonality(
            product1, factor: 1m, periodStartDate: periodStartDate);
        await _seasonalityHelper.CreateSeasonality(
            product2, factor: 1.3m, periodStartDate: periodStartDate);

        int count = await Fixture.Context.Seasonality
            .CountAsync(s =>
                s.PeriodStartDate == periodStartDate &&
                (s.ProductId == product1.Id || s.ProductId == product2.Id));

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // See DeleteProduct_DeletesAssociatedSeasonality inside ProductConfigurationTest.cs

    // -------------------------------------------------------
    // Factor precision (decimal(4,2) — max 99.99 storable but constrained to <= 12)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_FactorStoredWithTwoDecimalPlaces_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var seasonality = await _seasonalityHelper.CreateSeasonality(
            product, factor: 1.25m);

        Fixture.Context.ChangeTracker.Clear();

        var saved = await Fixture.Context.Seasonality
            .FirstAsync(s =>
                s.ProductId == product.Id &&
                s.PeriodStartDate == seasonality.PeriodStartDate);

        Assert.Equal(1.25m, saved.Factor);
    }
}