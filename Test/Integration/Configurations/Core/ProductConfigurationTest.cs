using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class ProductConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly InventoryPolicyHelper _inventoryPolicyHelper;
    private readonly StockLevelHelper _stockLevelHelper;
    private readonly PurchaseForecastHelper _purchaseForecastHelper;
    private readonly SalesAggregateHelper _salesAggregateHelper;
    private readonly ForecastHelper _forecastHelper;
    private readonly SeasonalityHelper _seasonalityHelper;
    private readonly ConfirmedOrderAggregateHelper _confirmedOrderAggregateHelper;
    private readonly TransactionHelper _transactionHelper;
    private readonly PurchaseOrderHelper _purchaseOrderHelper;
    private readonly ConfirmedOrderHelper _confirmedOrderHelper;
    private readonly UserDefinedFieldHelper _userDefinedFieldHelper;

    public ProductConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _inventoryPolicyHelper = new InventoryPolicyHelper(fixture);
        _stockLevelHelper = new StockLevelHelper(fixture);
        _purchaseForecastHelper = new PurchaseForecastHelper(fixture);
        _salesAggregateHelper = new SalesAggregateHelper(fixture);
        _forecastHelper = new ForecastHelper(fixture);
        _seasonalityHelper = new SeasonalityHelper(fixture);
        _confirmedOrderAggregateHelper = new ConfirmedOrderAggregateHelper(fixture);
        _transactionHelper = new TransactionHelper(fixture);
        _purchaseOrderHelper = new PurchaseOrderHelper(fixture);
        _confirmedOrderHelper = new ConfirmedOrderHelper(fixture);
        _userDefinedFieldHelper = new UserDefinedFieldHelper(fixture);
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company, name: null!, sku: "SKU-001", 
            price: 10, saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenSkuIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company, name: "Product", sku: null!, 
            price: 10, saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            name: new string('A', 51),
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            name: new string('A', 50));

        Assert.True(await Fixture.Context.Products
            .AnyAsync(x => x.Id == product.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenSkuExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            name: "Test",
            sku: new string('A', 51),
            price: 10,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenSkuIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            sku: new string('A', 50));

        Assert.True(await Fixture.Context.Products
            .AnyAsync(x => x.Id == product.Id));
    }

    // -------------------------------------------------------
    // Check constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenPriceIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            name: "Product",
            sku: "SKU-001",
            price: -1,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenPriceIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var product = await _productHelper.CreateProduct(
            company,
            price: 0);

        Assert.True(await Fixture.Context.Products
            .AnyAsync(x => x.Id == product.Id));
    }

    // -------------------------------------------------------
    // Unique indexes
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateSkuWithinCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(company, sku: "SKU-001");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _productHelper.CreateProduct(
                company,
                sku: "SKU-001",
                name: "Different Product"));
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateNameWithinCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(company, name: "Product");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _productHelper.CreateProduct(
                company,
                name: "Product",
                sku: "SKU-002"));
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateSkuAcrossCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(company1, sku: "SKU-001");
        await _productHelper.CreateProduct(company2, sku: "SKU-001");

        var count = await Fixture.Context.Products.CountAsync(x => x.SKU == "SKU-001");

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateNameAcrossCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(company1, name: "Product");
        await _productHelper.CreateProduct(company2, name: "Product");

        var count = await Fixture.Context.Products.CountAsync(x => x.Name == "Product");

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteProduct_DeletesAssociatedInventoryPolicicy()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _inventoryPolicyHelper.CreateInventoryPolicy(product);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.ProductInventoryPolicies
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedStockLevel()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _stockLevelHelper.CreateStockLevel(product);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.StockLevels
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedPurchaseForecasts()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _purchaseForecastHelper.CreatePurchaseForecast(product);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.PurchaseForecasts
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedSalesAggregates()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _salesAggregateHelper.CreateSalesAggregate(product, period: Period.Weekly);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.SalesAggregates
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedForecasts()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _forecastHelper.CreateForecast(product, period: Period.Weekly);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.Forecasts
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedSeasonality()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);
        
        await _seasonalityHelper.CreateSeasonality(product, factor: 1);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.Seasonality
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedConfirmedOrderAggregates()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _confirmedOrderAggregateHelper.CreateConfirmedOrderAggregate(product, period: Period.Weekly);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.ConfirmedOrderAggregates
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task DeleteProduct_DeletesAssociatedUserDefinedFields()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _userDefinedFieldHelper.CreateUserDefinedField(product);

        Fixture.Context.Products.Remove(product);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.UserDefinedFields
            .AnyAsync(x => x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Restrict delete
    // -------------------------------------------------------
    // see DeleteCompany_WhenProductsExist_ThrowsException inside CompanyConfigurationTest.cs

    [Fact]
    public async Task DeleteProduct_WhenTransactionLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company, DateTime.UtcNow.Date.AddDays(-2));

        await _transactionHelper.CreateTransactionLineItem(transaction, product, 100, 10);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }

    [Fact]
    public async Task DeleteProduct_WhenPurchaseOrderLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var po = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await _purchaseOrderHelper.CreatePurchaseOrderLineItem(po, product);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }

    [Fact]
    public async Task DeleteProduct_WhenConfirmedOrderLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var co = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await _confirmedOrderHelper.CreateConfirmedOrderLineItem(co, product);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }
}