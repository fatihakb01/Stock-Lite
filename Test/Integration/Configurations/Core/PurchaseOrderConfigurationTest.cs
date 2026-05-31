using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class PurchaseOrderConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly PurchaseOrderHelper _purchaseOrderHelper;
    private readonly ProductHelper _productHelper;

    public PurchaseOrderConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _purchaseOrderHelper = new PurchaseOrderHelper(fixture);
        _productHelper = new ProductHelper(fixture);
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenOrderNumberIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(
            company,
            orderNumber: null!,
            expectedDeliveryDate: DateTime.UtcNow.AddDays(7),
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenOrderNumberExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(
            company,
            orderNumber: new string('A', 51),
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenOrderNumberIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(
            company,
            orderNumber: new string('A', 50));

        Assert.True(await Fixture.Context.PurchaseOrder
            .AnyAsync(x => x.Id == order.Id));
    }

    // -------------------------------------------------------
    // Unique index
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateOrderNumberWithinCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(
            company,
            orderNumber: "PO-0001");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _purchaseOrderHelper.CreatePurchaseOrder(
                company,
                orderNumber: "PO-0001"));

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateOrderNumberAcrossCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(
            company1,
            orderNumber: "PO-0001");

        await _purchaseOrderHelper.CreatePurchaseOrder(
            company2,
            orderNumber: "PO-0001");

        var count = await Fixture.Context.PurchaseOrder
            .CountAsync(x => x.OrderNumber == "PO-0001");

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeletePurchaseOrder_DeletesAssociatedLineItems()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _purchaseOrderHelper.CreatePurchaseOrder(company);

        await _purchaseOrderHelper.CreatePurchaseOrderLineItem(
            order,
            product);

        Fixture.Context.PurchaseOrder.Remove(order);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.PurchaseOrderLineItem
            .AnyAsync(x =>
                x.PurchaseOrderId == order.Id &&
                x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Restrict delete
    // -------------------------------------------------------
    // Similar coverage already exists in CompanyConfigurationTest:
    // DeleteCompany_WhenPurchaseOrdersExist_ThrowsException

    [Fact]
    public async Task DeleteCompany_WhenPurchaseOrdersExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(company);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));

        Fixture.Context.ChangeTracker.Clear();
    }
}