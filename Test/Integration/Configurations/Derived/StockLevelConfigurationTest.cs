using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class StockLevelConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly StockLevelHelper _stockLevelHelper;

    public StockLevelConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _stockLevelHelper = new StockLevelHelper(fixture);
    }

    // -------------------------------------------------------
    // QuantityOnHand constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenQuantityOnHandIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _stockLevelHelper.CreateStockLevel(
                product,
                quantityOnHand: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityOnHandIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            quantityOnHand: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
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
            _stockLevelHelper.CreateStockLevel(
                product,
                quantityOnOrder: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityOnOrderIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            quantityOnOrder: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
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
            _stockLevelHelper.CreateStockLevel(
                product,
                quantityCommitted: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityCommittedIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            quantityCommitted: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
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
            _stockLevelHelper.CreateStockLevel(
                product,
                forecastQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenForecastQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            forecastQuantity: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
    }

    // -------------------------------------------------------
    // ReorderPoint constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenReorderPointIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _stockLevelHelper.CreateStockLevel(
                product,
                reorderPoint: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenReorderPointIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            reorderPoint: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
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
            _stockLevelHelper.CreateStockLevel(
                product,
                recommendedOrderQuantity: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenRecommendedOrderQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var stockLevel = await _stockLevelHelper.CreateStockLevel(
            product,
            recommendedOrderQuantity: 0);

        Assert.True(await Fixture.Context.StockLevels.AnyAsync(x =>
            x.ProductId == stockLevel.ProductId));
    }

    // -------------------------------------------------------
    // Primary key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateProductId_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _stockLevelHelper.CreateStockLevel(product);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _stockLevelHelper.CreateStockLevel(product));
    }

    [Fact]
    public async Task SaveChanges_WhenDifferentProducts_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product1 = await _productHelper.CreateProduct(
            company, name: "Product 1", sku: "SKU-0001");
        var product2 = await _productHelper.CreateProduct(
            company, name: "Product 2", sku: "SKU-0002");

        await _stockLevelHelper.CreateStockLevel(product1);
        await _stockLevelHelper.CreateStockLevel(product2);

        int count = await Fixture.Context.StockLevels
            .CountAsync(s => (s.ProductId == product1.Id || s.ProductId == product2.Id));

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedStockLevel inside ProductConfigurationTest.cs
}