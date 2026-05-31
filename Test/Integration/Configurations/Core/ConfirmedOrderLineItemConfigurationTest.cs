using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class ConfirmedOrderLineItemConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ConfirmedOrderHelper _confirmedOrderHelper;
    private readonly ProductHelper _productHelper;

    public ConfirmedOrderLineItemConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _confirmedOrderHelper = new ConfirmedOrderHelper(fixture);
        _productHelper = new ProductHelper(fixture);
    }

    // -------------------------------------------------------
    // Quantity constraint (> 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenQuantityIsZero_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _confirmedOrderHelper.CreateConfirmedOrderLineItem(
                order,
                product,
                quantity: 0,
                soldQuantity: 0,
                unitCost: 10));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityIsValid_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        var lineItem = await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: 10,
            soldQuantity: 0,
            unitCost: 10);

        Assert.True(await Fixture.Context.ConfirmedOrderLineItem.AnyAsync(x =>
            x.ConfirmedOrderId == order.Id &&
            x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // SoldQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenSoldQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _confirmedOrderHelper.CreateConfirmedOrderLineItem(
                order,
                product,
                quantity: 10,
                soldQuantity: -1,
                unitCost: 10));
    }

    [Fact]
    public async Task SaveChanges_WhenSoldQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        var lineItem = await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: 10,
            soldQuantity: 0,
            unitCost: 10);

        Assert.True(await Fixture.Context.ConfirmedOrderLineItem.AnyAsync(x =>
            x.ConfirmedOrderId == order.Id &&
            x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // UnitCost constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenUnitCostIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _confirmedOrderHelper.CreateConfirmedOrderLineItem(
                order,
                product,
                quantity: 10,
                soldQuantity: 0,
                unitCost: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenUnitCostIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        var lineItem = await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: 10,
            soldQuantity: 0,
            unitCost: 0);

        Assert.True(await Fixture.Context.ConfirmedOrderLineItem.AnyAsync(x =>
            x.ConfirmedOrderId == order.Id &&
            x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Composite key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateOrderProductPair_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: 10,
            soldQuantity: 0,
            unitCost: 10);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _confirmedOrderHelper.CreateConfirmedOrderLineItem(
                order,
                product,
                quantity: 5,
                soldQuantity: 0,
                unitCost: 10));
    }

    // -------------------------------------------------------
    // Cascade delete - Product
    // -------------------------------------------------------
    // see DeleteConfirmedOrder_DeletesAssociatedLineItems inside ConfirmedOrderConfigurationTest.cs

    // -------------------------------------------------------
    // Restrict delete - Product
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteProduct_WhenLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product,
            quantity: 10,
            soldQuantity: 0,
            unitCost: 10);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }
}