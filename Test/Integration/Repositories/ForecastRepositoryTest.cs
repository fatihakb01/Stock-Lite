using Domain.Enums;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories;

public class ForecastRepositoryTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ForecastHelper _forecastHelper = new ForecastHelper(fixture);
    private readonly DateTime _now = DateTime.UtcNow.Date;

    private ForecastRepository CreateRepository() =>
        new(Fixture.Context);

    public static IEnumerable<object?[]> InsertPeriodData =>
    [
        [Period.Daily, 30],
        [Period.Weekly, 53],
        [Period.Monthly, 13],
        [null, 96]
    ];

    public static IEnumerable<object[]> UpdatePeriodData =>
    [
        [Period.Daily],
        [Period.Weekly],
        [Period.Monthly]
    ];

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetForecastByProductAsync_WhenPeriodAndForecast_ReturnsForecast(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);

        var forecast = ForecastHelper.CreateForecast(product, period);
        await Fixture.Context.Forecasts.AddAsync(forecast);
        await Fixture.Context.SaveChangesAsync();

        var result = await repo.GetForecastByProductAsync(product.Id, period);

        Assert.NotEmpty(result);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task GetForecastByProductAsync_WhenPeriodAndNoForecast_ReturnsForecast(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        var result = await repo.GetForecastByProductAsync(product.Id, period);

        Assert.Empty(result);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertForecastsAsync_WhenPeriod_InsertsRows(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _forecastHelper.GetForecastCountAsync(product.Id, period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertForecastsAsync_WhenCalledTwice_DoesNotDuplicate(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company.Id,
            [product.Id],
            period);

        await repo.InsertForecastsAsync(
            company.Id,
            [product.Id],
            period);

        int count = await _forecastHelper.GetForecastCountAsync(product.Id, period);

        Assert.Equal(rows, count);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertForecastsAsync_WhenCompanySpecified_OnlyInsertsForThatCompany(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = ProductHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(company1.Id, [product1.Id], period);

        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertForecastsAsync_WhenProductIdsNotSpecified_InsertsForAllProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(company.Id, [], period);

        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(rows, count2);
    }

    [Theory]
    [MemberData(nameof(InsertPeriodData))]
    public async Task InsertForecastsAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts(
        Period? period,
        int rows)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(company.Id, [product1.Id], period);

        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(rows, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteForecastsAsync_DeletesPastForecasts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        // Seed a past forecast — PeriodEndDate in the past
        var pastForecast = ForecastHelper.CreateForecast(
            product,
            period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await Fixture.Context.Forecasts.AddAsync(pastForecast);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeleteForecastsAsync(company.Id, [product.Id], period);
        int count = await _forecastHelper.GetForecastCountAsync(product.Id, period);

        Assert.Equal(0, count);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteForecastsAsync_DoesNotDeleteFutureForecasts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        // Future forecast — should survive deletion
        var futureForecast = ForecastHelper.CreateForecast(
            product,
            period,
            periodStartDate: _now,
            periodEndDate: _now.AddMonths(1));

        await Fixture.Context.Forecasts.AddAsync(futureForecast);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeleteForecastsAsync(company.Id, [product.Id], period);
        int count = await _forecastHelper.GetForecastCountAsync(product.Id, period);

        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteForecastsAsync_WhenProductIdsSpecified_OnlyDeletesSpecifiedProducts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var pastForecast1 = ForecastHelper.CreateForecast(
            product1, period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        var pastForecast2 = ForecastHelper.CreateForecast(
            product2, period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await Fixture.Context.Forecasts.AddRangeAsync(pastForecast1, pastForecast2);
        await Fixture.Context.SaveChangesAsync();

        // Only delete for product1
        await repo.DeleteForecastsAsync(company.Id, [product1.Id], period);
        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteForecastsAsync_WhenProductIdsNotSpecified_DeletesAllProducts(Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var pastForecast1 = ForecastHelper.CreateForecast(
            product1, period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        var pastForecast2 = ForecastHelper.CreateForecast(
            product2, Period.Monthly,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await Fixture.Context.Forecasts.AddRangeAsync(pastForecast1, pastForecast2);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeleteForecastsAsync(company.Id, [], period);
        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(0, count1);
        Assert.Equal(0, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task DeleteForecastsAsync_WhenCompanySpecified_OnlyDeletesThatCompany(Period period)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = ProductHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var pastForecast1 = ForecastHelper.CreateForecast(
            product1, period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        var pastForecast2 = ForecastHelper.CreateForecast(
            product2, period,
            periodStartDate: _now.AddMonths(-2),
            periodEndDate: _now.AddMonths(-1));

        await Fixture.Context.Forecasts.AddRangeAsync(pastForecast1, pastForecast2);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeleteForecastsAsync(company1.Id, [product1.Id], period);
        int count1 = await _forecastHelper.GetForecastCountAsync(product1.Id, period);
        int count2 = await _forecastHelper.GetForecastCountAsync(product2.Id, period);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateForecastsAsync_WhenPeriod_UpdatesForecastValues(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        // monthly sales needed for forecast calculation
        var aggregate = SalesAggregateHelper.CreateSalesAggregate(product, Period.Monthly);
        await Fixture.Context.SalesAggregates.AddAsync(aggregate);

        var seasonality = SeasonalityHelper.CreateSeasonality(product, 1);
        await Fixture.Context.Seasonality.AddAsync(seasonality);
        await Fixture.Context.SaveChangesAsync();

        // Call with period = null to make sure all forecast is computed
        await repo.InsertForecastsAsync(
            company.Id,
            [product.Id],
            null);

        await repo.UpdateForecastsAsync(
            company.Id, 
            [product.Id], 
            null
        );

        var forecast = await _forecastHelper.GetForecastAsync(product.Id, period);

        Assert.True(forecast.PredictedQuantity > 0);
        Assert.True(forecast.PredictedRevenue > 0);
        Assert.Equal(0.50, forecast.Confidence);
    }

    [Fact]
    public async Task UpdateForecastsAsync_WhenNoPeriod_UpdatesAllForecastValues()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        var aggregate = SalesAggregateHelper.CreateSalesAggregate(product, Period.Monthly);
        await Fixture.Context.SalesAggregates.AddAsync(aggregate);

        var seasonality = SeasonalityHelper.CreateSeasonality(product, 1);
        await Fixture.Context.Seasonality.AddAsync(seasonality);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company.Id,
            [product.Id],
            null);

        await repo.UpdateForecastsAsync(
            company.Id,
            [product.Id],
            null
        );

        var daily = await _forecastHelper.GetForecastAsync(product.Id, Period.Daily);
        var weekly = await _forecastHelper.GetForecastAsync(product.Id, Period.Weekly);
        var monthly = await _forecastHelper.GetForecastAsync(product.Id, Period.Monthly);

        Assert.True(daily.PredictedQuantity > 0);
        Assert.True(daily.PredictedRevenue > 0);
        Assert.Equal(0.50, daily.Confidence);

        Assert.True(weekly.PredictedQuantity > 0);
        Assert.True(weekly.PredictedRevenue > 0);
        Assert.Equal(0.50, weekly.Confidence);

        Assert.True(monthly.PredictedQuantity > 0);
        Assert.True(monthly.PredictedRevenue > 0);
        Assert.Equal(0.50, monthly.Confidence);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateForecastsAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = ProductHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        await _forecastHelper.SeedForecastDependenciesAsync(product1, 100, 1000, Period.Monthly);
        await _forecastHelper.SeedForecastDependenciesAsync(product2, 200, 2000, Period.Monthly);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company.Id,
            [product1.Id, product2.Id],
            null);

        await repo.UpdateForecastsAsync(
            company.Id,
            [product1.Id],
            null);

        var forecast1 = await _forecastHelper.GetForecastAsync(product1.Id, period);
        var forecast2 = await _forecastHelper.GetForecastAsync(product2.Id, period);

        Assert.True(forecast1.PredictedQuantity > 0);
        Assert.True(forecast1.PredictedRevenue > 0);

        Assert.Equal(0, forecast2.PredictedQuantity);
        Assert.Equal(0, forecast2.PredictedRevenue);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateForecastsAsync_WhenProductIdsNotSpecified_UpdatesAllProducts(
        Period period)
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(
            company,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = ProductHelper.CreateProduct(
            company,
            name: "Product 2",
            sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        await _forecastHelper.SeedForecastDependenciesAsync(product1, 100, 1000, Period.Monthly);
        await _forecastHelper.SeedForecastDependenciesAsync(product2, 200, 2000, Period.Monthly);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company.Id,
            [],
            null);

        await repo.UpdateForecastsAsync(
            company.Id,
            [],
            null);

        var forecast1 = await _forecastHelper.GetForecastAsync(product1.Id, period);
        var forecast2 = await _forecastHelper.GetForecastAsync(product2.Id, period);

        Assert.True(forecast1.PredictedQuantity > 0);
        Assert.True(forecast1.PredictedRevenue > 0);

        Assert.True(forecast2.PredictedQuantity > 0);
        Assert.True(forecast2.PredictedRevenue > 0);
    }

    [Theory]
    [MemberData(nameof(UpdatePeriodData))]
    public async Task UpdateForecastsAsync_WhenCompanySpecified_OnlyUpdatesThatCompany(
        Period period)
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = ProductHelper.CreateProduct(
            company1,
            name: "Product 1",
            sku: "SKU-0001");

        var product2 = ProductHelper.CreateProduct(
            company2,
            name: "Product 2",
            sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        await _forecastHelper.SeedForecastDependenciesAsync(product1, 100, 1000, Period.Monthly);
        await _forecastHelper.SeedForecastDependenciesAsync(product2, 200, 2000, Period.Monthly);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertForecastsAsync(
            company1.Id,
            [product1.Id],
            null);

        await repo.InsertForecastsAsync(
            company2.Id,
            [product2.Id],
            null);

        await repo.UpdateForecastsAsync(
            company1.Id,
            [product1.Id],
            null);

        var forecast1 = await _forecastHelper.GetForecastAsync(product1.Id, period);
        var forecast2 = await _forecastHelper.GetForecastAsync(product2.Id, period);

        Assert.True(forecast1.PredictedQuantity > 0);
        Assert.True(forecast1.PredictedRevenue > 0);

        Assert.Equal(0, forecast2.PredictedQuantity);
        Assert.Equal(0, forecast2.PredictedRevenue);
    }
}