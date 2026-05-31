using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class ProductInventoryPolicyConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductHelper _productHelper;
    private readonly InventoryPolicyHelper _inventoryPolicyHelper;

    public ProductInventoryPolicyConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productHelper = new ProductHelper(fixture);
        _inventoryPolicyHelper = new InventoryPolicyHelper(fixture);
    }

    // -------------------------------------------------------
    // LeadTimeDays constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenLeadTimeDaysIsZero_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            leadTimeDays: 0,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenLeadTimeDaysIsGreaterThan365_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            leadTimeDays: 366,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenLeadTimeDaysIsOne_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            leadTimeDays: 1);

        Assert.True(await Fixture.Context.ProductInventoryPolicies
            .AnyAsync(x => x.ProductId == product.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenLeadTimeDaysIs365_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            leadTimeDays: 365);

        Assert.True(await Fixture.Context.ProductInventoryPolicies
            .AnyAsync(x => x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // MinimumOrderQuantity constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenMinimumOrderQuantityIsZero_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            minimumOrderQuantity: 0,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenMinimumOrderQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            minimumOrderQuantity: -1,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenMinimumOrderQuantityIsOne_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            minimumOrderQuantity: 1);

        Assert.True(await Fixture.Context.ProductInventoryPolicies
            .AnyAsync(x => x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // OrderIncrementQuantity constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenOrderIncrementQuantityIsZero_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            orderIncrementQuantity: 0,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenOrderIncrementQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            orderIncrementQuantity: -1,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenOrderIncrementQuantityIsOne_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = await _inventoryPolicyHelper.CreateInventoryPolicy(
            product,
            orderIncrementQuantity: 1);

        Assert.True(await Fixture.Context.ProductInventoryPolicies
            .AnyAsync(x => x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Default values
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenSafetyStockNotSpecified_DefaultsToZero()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = new ProductInventoryPolicy
        {
            ProductId = product.Id,
            Product = product,
            LeadTimeDays = 7,
            MinimumOrderQuantity = 1,
            OrderIncrementQuantity = 1
        };

        Fixture.Context.ProductInventoryPolicies.Add(policy);
        await Fixture.Context.SaveChangesAsync();

        Fixture.Context.ChangeTracker.Clear();

        var result = await Fixture.Context.ProductInventoryPolicies
            .SingleAsync(x => x.ProductId == product.Id);

        Assert.Equal(0, result.SafetyStock);
    }

    [Fact]
    public async Task SaveChanges_WhenStockAdjustmentNotSpecified_DefaultsToZero()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var policy = new ProductInventoryPolicy
        {
            ProductId = product.Id,
            Product = product,
            LeadTimeDays = 7,
            MinimumOrderQuantity = 1,
            OrderIncrementQuantity = 1
        };

        Fixture.Context.ProductInventoryPolicies.Add(policy);
        await Fixture.Context.SaveChangesAsync();

        Fixture.Context.ChangeTracker.Clear();

        var result = await Fixture.Context.ProductInventoryPolicies
            .SingleAsync(x => x.ProductId == product.Id);

        Assert.Equal(0, result.StockAdjustment);
    }
}