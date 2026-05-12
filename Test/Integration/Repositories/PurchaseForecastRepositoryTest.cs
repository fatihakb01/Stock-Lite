using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories;

public class PurchaseForecastRepositoryTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly PurchaseForecastHelper _purchaseForecastHelper = new PurchaseForecastHelper(fixture);
    private readonly ForecastHelper _forecastHelper = new(fixture);
    private readonly DateTime _now = DateTime.UtcNow.Date;

    private PurchaseForecastRepository CreateRepository() => new PurchaseForecastRepository(Fixture.Context);

    // Seeds the minimum data needed for Insert to produce rows
    private async Task<(Company, Product)> SeedInsertPrerequisitesAsync(
        string companyName = "Test Company",
        string productName = "Test Product",
        string sku = "SKU-0001")
    {
        var company = await _companyHelper.CreateCompany(name: companyName);
        var product = ProductHelper.CreateProduct(company, name: productName, sku: sku);

        
        await Fixture.Context.Products.AddAsync(product);
        await _forecastHelper.SeedWeeklyForecastsAsync(product);
        await Fixture.Context.SaveChangesAsync();

        return (company, product);
    }

    // Seeds everything needed for Update to produce meaningful results
    private async Task<(Company, Product)> SeedUpdatePrerequisitesAsync(
        string companyName = "Test Company",
        string productName = "Test Product",
        string sku = "SKU-0001",
        int safetyStock = 10,
        int leadTimeDays = 7,
        int moq = 10,
        int orderIncrement = 10,
        int quantityOnHand = 0,
        int quantityOnOrder = 0,
        int quantityCommitted = 0,
        int netStockPosition = 10,
        int forecastQuantity = 10,
        int reorderPoint = 1,
        bool needsReorder = false,
        int recommendedOrderQuantity = 0)
    {
        var company = await _companyHelper.CreateCompany(name: companyName);
        var product = ProductHelper.CreateProduct(company, name: productName, sku: sku);

        
        await Fixture.Context.Products.AddAsync(product);
        await _forecastHelper.SeedWeeklyForecastsAsync(product, forecastQuantity);

        var stocklevel = StockLevelHelper.CreateStockLevel(
            product: product,
            quantityOnHand: quantityOnHand,
            quantityOnOrder: quantityOnOrder,
            quantityCommitted: quantityCommitted,
            netStockPosition: netStockPosition,
            forecastQuantity: forecastQuantity,
            reorderPoint: reorderPoint,
            needsReorder: needsReorder,
            recommendedOrderQuantity: recommendedOrderQuantity
        );

        var policy = InventoryPolicyHelper.CreateInventoryPolicy(
            product: product,
            safetyStock: safetyStock,
            leadTimeDays: leadTimeDays,
            minimumOrderQuantity: moq,
            orderIncrementQuantity: orderIncrement);

        await Fixture.Context.ProductInventoryPolicies.AddAsync(policy);
        await Fixture.Context.StockLevels.AddAsync(stocklevel);
        await Fixture.Context.SaveChangesAsync();

        return (company, product);
    }

    [Fact]
    public async Task GetByProductAsync_WhenPurchaseForecastExists_ReturnsForecasts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        var purchaseForecast = PurchaseForecastHelper.CreatePurchaseForecast(product);
        await Fixture.Context.PurchaseForecasts.AddAsync(purchaseForecast);
        await Fixture.Context.SaveChangesAsync();

        var result = await repo.GetByProductAsync(product.Id);

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByProductAsync_WhenNoPurchaseForecastExists_ReturnsEmpty()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);
        await Fixture.Context.SaveChangesAsync();

        var result = await repo.GetByProductAsync(product.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task InsertPurchaseForecastsAsync_InsertsOneRowPerWeek()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedInsertPrerequisitesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);

        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product.Id);

        Assert.Equal(52, count);
    }

    [Fact]
    public async Task InsertPurchaseForecastsAsync_WhenCalledTwice_DoesNotDuplicate()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedInsertPrerequisitesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);

        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product.Id);

        Assert.Equal(52, count);
    }

    [Fact]
    public async Task InsertPurchaseForecastsAsync_WhenCompanySpecified_OnlyInsertsForThatCompany()
    {
        var repo = CreateRepository();
        var (company1, product1) = await SeedInsertPrerequisitesAsync(
            "Company 1", "Product 1", "SKU-0001");
        var (company2, product2) = await SeedInsertPrerequisitesAsync(
            "Company 2", "Product 2", "SKU-0002");

        await repo.InsertPurchaseForecastsAsync(company1.Id, [product1.Id]);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(52, count1);
        Assert.Equal(0, count2);
    }

    [Fact]
    public async Task InsertPurchaseForecastsAsync_WhenProductIdsNotSpecified_InsertsForAllProducts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await _forecastHelper.SeedWeeklyForecastsAsync(product1);
        await _forecastHelper.SeedWeeklyForecastsAsync(product2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, []);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(52, count1);
        Assert.Equal(52, count2);
    }

    [Fact]
    public async Task InsertPurchaseForecastsAsync_WhenProductIdsSpecified_OnlyInsertsForSpecifiedProducts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await _forecastHelper.SeedWeeklyForecastsAsync(product1);
        await _forecastHelper.SeedWeeklyForecastsAsync(product2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product1.Id]);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(52, count1);
        Assert.Equal(0, count2);
    }

    [Fact]
    public async Task DeletePurchaseForecastsAsync_DeletesPastForecasts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        // Past forecast
        var past = PurchaseForecastHelper.CreatePurchaseForecast(
            product,
            weekStartDate: _now.AddDays(-14),
            weekEndDate: _now.AddDays(-8));

        await Fixture.Context.PurchaseForecasts.AddAsync(past);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeletePurchaseForecastsAsync(company.Id, [product.Id]);
        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product.Id);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task DeletePurchaseForecastsAsync_DoesNotDeleteFutureForecasts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();
        var product = ProductHelper.CreateProduct(company);

        
        await Fixture.Context.Products.AddAsync(product);

        // Future forecast
        var future = PurchaseForecastHelper.CreatePurchaseForecast(
            product,
            weekStartDate: _now,
            weekEndDate: _now.AddDays(6));

        await Fixture.Context.PurchaseForecasts.AddAsync(future);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeletePurchaseForecastsAsync(company.Id, [product.Id]);
        int count = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product.Id);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task DeletePurchaseForecastsAsync_WhenProductIdsSpecified_OnlyDeletesSpecifiedProducts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var past1 = PurchaseForecastHelper.CreatePurchaseForecast(
            product1, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));
        var past2 = PurchaseForecastHelper.CreatePurchaseForecast(
            product2, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));

        await Fixture.Context.PurchaseForecasts.AddRangeAsync(past1, past2);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeletePurchaseForecastsAsync(company.Id, [product1.Id]);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public async Task DeletePurchaseForecastsAsync_WhenProductIdsNotSpecified_DeletesAllProducts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var past1 = PurchaseForecastHelper.CreatePurchaseForecast(
            product1, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));
        var past2 = PurchaseForecastHelper.CreatePurchaseForecast(
            product2, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));

        await Fixture.Context.PurchaseForecasts.AddRangeAsync(past1, past2);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeletePurchaseForecastsAsync(company.Id, []);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(0, count1);
        Assert.Equal(0, count2);
    }

    [Fact]
    public async Task DeletePurchaseForecastsAsync_WhenCompanySpecified_OnlyDeletesThatCompany()
    {
        var repo = CreateRepository();
        var company1 = await _companyHelper.CreateCompany(name: "Company 1");
        var company2 = await _companyHelper.CreateCompany(name: "Company 2");

        var product1 = ProductHelper.CreateProduct(company1, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company2, name: "Product 2", sku: "SKU-0002");

        await Fixture.Context.Products.AddRangeAsync(product1, product2);

        var past1 = PurchaseForecastHelper.CreatePurchaseForecast(
            product1, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));
        var past2 = PurchaseForecastHelper.CreatePurchaseForecast(
            product2, weekStartDate: _now.AddDays(-14), weekEndDate: _now.AddDays(-8));

        await Fixture.Context.PurchaseForecasts.AddRangeAsync(past1, past2);
        await Fixture.Context.SaveChangesAsync();

        await repo.DeletePurchaseForecastsAsync(company1.Id, [product1.Id]);

        int count1 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product1.Id);
        int count2 = await Fixture.Context.PurchaseForecasts
            .CountAsync(x => x.ProductId == product2.Id);

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_SetsProjectedStockAndForecastQuantity()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedUpdatePrerequisitesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

        var forecast = await _purchaseForecastHelper.GetPurchaseForecastAsync(product.Id);

        Assert.NotNull(forecast);
        Assert.True(forecast.ForecastQuantity > 0);
        Assert.True(forecast.ProjectedStock > 0 || forecast.ProjectedStock < 0);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WithLowStock_SetsNeedsReorderTrue()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedUpdatePrerequisitesAsync(
            safetyStock: 100, 
            quantityOnHand: 0, 
            reorderPoint: 10);

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

        var forecasts = await Fixture.Context.PurchaseForecasts
            .Where(x => x.ProductId == product.Id)
            .ToListAsync();

        Assert.NotEmpty(forecasts);
        Assert.Contains(forecasts, f => f.NeedsReorder);
        Assert.Contains(forecasts, f => f.RecommendedOrderQuantity > 0);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WithSufficientStock_NeedsReorderIsFalse()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedUpdatePrerequisitesAsync(
            safetyStock: 0,
            quantityOnHand: 0,
            reorderPoint: 10,
            leadTimeDays: 7);

        // Add enough received purchase orders to create high QuantityOnHand
        var po = PurchaseOrderHelper.CreatePurchaseOrder(
            company, 
            status: PurchaseOrderStatus.Completed,
            expectedDeliveryDate: _now,
            receivedDate: _now);
        await Fixture.Context.PurchaseOrder.AddAsync(po);

        var poLine = PurchaseOrderHelper.CreatePurchaseOrderLineItem(
            po, product, quantity: 1000, receivedQuantity: 1000);
        await Fixture.Context.PurchaseOrderLineItem.AddAsync(poLine);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

        var forecasts = await _purchaseForecastHelper.GetPurchaseForecastsAsync(product.Id);

        Assert.All(forecasts, f => Assert.False(f.NeedsReorder));
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WithOpenPurchaseOrders_ReflectsQuantityOnOrder()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedUpdatePrerequisitesAsync();

        // Open purchase order arriving this week
        var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
        var po = PurchaseOrderHelper.CreatePurchaseOrder(
            company,
            expectedDeliveryDate: startDate.AddDays(3),
            status: PurchaseOrderStatus.Draft);
        await Fixture.Context.PurchaseOrder.AddAsync(po);

        var poLine = PurchaseOrderHelper.CreatePurchaseOrderLineItem(
            po, product, quantity: 50, receivedQuantity: 0);
        await Fixture.Context.PurchaseOrderLineItem.AddAsync(poLine);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

        var currentWeekForecast = await _purchaseForecastHelper.GetPurchaseForecastAsync(product.Id);

        Assert.NotNull(currentWeekForecast);
        Assert.Equal(50, currentWeekForecast!.QuantityOnOrder);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WithOpenConfirmedOrders_ReflectsQuantityCommitted()
    {
        var repo = CreateRepository();
        var (company, product) = await SeedUpdatePrerequisitesAsync();

        // Open confirmed order arriving this week
        var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
        var co = ConfirmedOrderHelper.CreateConfirmedOrder(
            company,
            expectedDeliveryDate: startDate.AddDays(3),
            status: ConfirmedOrderStatus.Confirmed);
        await Fixture.Context.ConfirmedOrder.AddAsync(co);

        var coLine = ConfirmedOrderHelper.CreateConfirmedOrderLineItem(
            co, product, quantity: 50, soldQuantity: 0);
        await Fixture.Context.ConfirmedOrderLineItem.AddAsync(coLine);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product.Id]);

        var currentWeekForecast = await _purchaseForecastHelper.GetPurchaseForecastAsync(product.Id);

        Assert.NotNull(currentWeekForecast);
        Assert.Equal(50, currentWeekForecast!.QuantityCommitted);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WhenProductIdsSpecified_OnlyUpdatesSpecifiedProducts()
    {
        var repo = CreateRepository();
        var company = await _companyHelper.CreateCompany();

        var product1 = ProductHelper.CreateProduct(company, name: "Product 1", sku: "SKU-0001");
        var product2 = ProductHelper.CreateProduct(company, name: "Product 2", sku: "SKU-0002");

        
        await Fixture.Context.Products.AddRangeAsync(product1, product2);
        await _forecastHelper.SeedWeeklyForecastsAsync(product1);
        await _forecastHelper.SeedWeeklyForecastsAsync(product2);

        var policy1 = InventoryPolicyHelper.CreateInventoryPolicy(product1, safetyStock: 100);
        var policy2 = InventoryPolicyHelper.CreateInventoryPolicy(product2, safetyStock: 100);
        await Fixture.Context.ProductInventoryPolicies.AddRangeAsync(policy1, policy2);
        await Fixture.Context.SaveChangesAsync();

        await repo.InsertPurchaseForecastsAsync(company.Id, [product1.Id, product2.Id]);
        await repo.UpdatePurchaseForecastsAsync(company.Id, [product1.Id]);

        var forecasts1 = await Fixture.Context.PurchaseForecasts
            .Where(x => x.ProductId == product1.Id).ToListAsync();
        var forecasts2 = await Fixture.Context.PurchaseForecasts
            .Where(x => x.ProductId == product2.Id).ToListAsync();

        Assert.Contains(forecasts1, f => f.ProjectedStock > 0 || f.ProjectedStock < 0);
        Assert.All(forecasts2, f => f.ProjectedStock = 0);
    }

    [Fact]
    public async Task UpdatePurchaseForecastsAsync_WhenCompanySpecified_OnlyUpdatesThatCompany()
    {
        var repo = CreateRepository();
        var (company1, product1) = await SeedUpdatePrerequisitesAsync(
            "Company 1", "Product 1", "SKU-0001");
        var (company2, product2) = await SeedUpdatePrerequisitesAsync(
            "Company 2", "Product 2", "SKU-0002");

        await repo.InsertPurchaseForecastsAsync(company1.Id, [product1.Id]);
        await repo.InsertPurchaseForecastsAsync(company2.Id, [product2.Id]);
        await repo.UpdatePurchaseForecastsAsync(company1.Id, [product1.Id]);

        var forecasts1 = await Fixture.Context.PurchaseForecasts
            .Where(x => x.ProductId == product1.Id).ToListAsync();
        var forecasts2 = await Fixture.Context.PurchaseForecasts
            .Where(x => x.ProductId == product2.Id).ToListAsync();

        Assert.Contains(forecasts1, f => f.ProjectedStock > 0 || f.ProjectedStock < 0);
        Assert.All(forecasts2, f => f.ProjectedStock = 0);
    }
}