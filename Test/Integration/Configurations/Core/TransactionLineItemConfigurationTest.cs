using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class TransactionLineItemConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly TransactionHelper _transactionHelper;
    private readonly ProductHelper _productHelper;

    public TransactionLineItemConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _transactionHelper = new TransactionHelper(fixture);
        _productHelper = new ProductHelper(fixture);
    }

    // -------------------------------------------------------
    // UnitPrice check constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenUnitPriceIsNegative_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _transactionHelper.CreateTransactionLineItem(
                transaction,
                product,
                quantity: 10,
                unitPrice: -1,
                saveChanges: true));
    }

    [Fact]
    public async Task SaveChanges_WhenUnitPriceIsZero_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date,
            saveChanges: false);

        var lineItem = await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 10,
            unitPrice: 0,
            saveChanges: true);

        Assert.True(await Fixture.Context.TransactionLineItems.AnyAsync(x =>
            x.TransactionId == transaction.Id &&
            x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Composite key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateTransactionProductPair_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 10,
            unitPrice: 5);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 5,
            unitPrice: 5));
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteTransaction_DeletesAssociatedLineItems inside TransactionConfigurationTest.cs

    // -------------------------------------------------------
    // Restrict delete (Product)
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteProduct_WhenLineItemsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date,
            saveChanges: false);

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 10,
            unitPrice: 5);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Products.Remove(product));
    }
}