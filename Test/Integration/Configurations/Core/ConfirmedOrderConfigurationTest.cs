using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class ConfirmedOrderConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ConfirmedOrderHelper _confirmedOrderHelper;
    private readonly ProductHelper _productHelper;

    public ConfirmedOrderConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _confirmedOrderHelper = new ConfirmedOrderHelper(fixture);
        _productHelper = new ProductHelper(fixture);
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenOrderNumberIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var order = _confirmedOrderHelper.CreateConfirmedOrder(
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

        await _confirmedOrderHelper.CreateConfirmedOrder(
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

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(
            company,
            orderNumber: new string('A', 50));

        Assert.True(await Fixture.Context.ConfirmedOrder
            .AnyAsync(x => x.Id == order.Id));
    }

    // -------------------------------------------------------
    // Unique index
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateOrderNumberWithinCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _confirmedOrderHelper.CreateConfirmedOrder(
            company,
            orderNumber: "CO-0001");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _confirmedOrderHelper.CreateConfirmedOrder(
                company,
                orderNumber: "CO-0001"));

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateOrderNumberAcrossCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _confirmedOrderHelper.CreateConfirmedOrder(
            company1,
            orderNumber: "CO-0001");

        await _confirmedOrderHelper.CreateConfirmedOrder(
            company2,
            orderNumber: "CO-0001");

        var count = await Fixture.Context.ConfirmedOrder
            .CountAsync(x => x.OrderNumber == "CO-0001");

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteConfirmedOrder_DeletesAssociatedLineItems()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var order = await _confirmedOrderHelper.CreateConfirmedOrder(company);

        var lineItem = await _confirmedOrderHelper.CreateConfirmedOrderLineItem(
            order,
            product);

        Fixture.Context.ConfirmedOrder.Remove(order);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.ConfirmedOrderLineItem
            .AnyAsync(x =>
                x.ConfirmedOrderId == order.Id &&
                x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Restrict delete
    // -------------------------------------------------------
    // Similar coverage already exists in CompanyConfigurationTest:
    // DeleteCompany_WhenConfirmedOrdersExist_ThrowsException

    [Fact]
    public async Task DeleteCompany_WhenConfirmedOrdersExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _confirmedOrderHelper.CreateConfirmedOrder(company);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));

        Fixture.Context.ChangeTracker.Clear();
    }
}