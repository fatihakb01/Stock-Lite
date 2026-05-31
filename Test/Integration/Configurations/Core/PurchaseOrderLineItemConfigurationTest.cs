using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class PurchaseOrderLineItemConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly PurchaseOrderHelper _purchaseOrderHelper;
    private readonly ProductHelper _productHelper;

    public PurchaseOrderLineItemConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _purchaseOrderHelper = new PurchaseOrderHelper(fixture);
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

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseOrderHelper.CreatePurchaseOrderLineItem(
                order,
                product,
                quantity: 0,
                receivedQuantity: 0,
                unitCost: 10));
    }

    [Fact]
    public async Task SaveChanges_WhenQuantityIsValid_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        var lineItem = await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product,
            quantity: 10,
            receivedQuantity: 0,
            unitCost: 10);

        Assert.True(await Fixture.Context.PurchaseOrderLineItem.AnyAsync(x =>
            x.PurchaseOrderId == order.Id &&
            x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // ReceivedQuantity constraint (>= 0)
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenReceivedQuantityIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(
            company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseOrderHelper.CreatePurchaseOrderLineItem(
                order,
                product,
                quantity: 10,
                receivedQuantity: -1,
                unitCost: 10));
    }

    [Fact]
    public async Task SaveChanges_WhenReceivedQuantityIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        var lineItem = await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product,
            quantity: 10,
            receivedQuantity: 0,
            unitCost: 10);

        Assert.True(await Fixture.Context.PurchaseOrderLineItem.AnyAsync(x =>
            x.PurchaseOrderId == order.Id &&
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

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseOrderHelper.CreatePurchaseOrderLineItem(
                order,
                product,
                quantity: 10,
                receivedQuantity: 0,
                unitCost: -1));
    }

    [Fact]
    public async Task SaveChanges_WhenUnitCostIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        var lineItem = await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product,
            quantity: 10,
            receivedQuantity: 0,
            unitCost: 0);

        Assert.True(await Fixture.Context.PurchaseOrderLineItem.AnyAsync(x =>
            x.PurchaseOrderId == order.Id &&
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

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product,
            quantity: 10,
            receivedQuantity: 0,
            unitCost: 10);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _purchaseOrderHelper.CreatePurchaseOrderLineItem(
                order,
                product,
                quantity: 5,
                receivedQuantity: 0,
                unitCost: 10));
    }

    // -------------------------------------------------------
    // Delete behavior - cascade from PurchaseOrder
    // -------------------------------------------------------
    // see DeletePurchaseOrder_DeletesAssociatedLineItems inside PurchaseOrderConfigurationTest.cs

    // -------------------------------------------------------
    // Restrict delete - Product
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteProduct_WhenLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product,
            quantity: 10,
            receivedQuantity: 0,
            unitCost: 10);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }
}