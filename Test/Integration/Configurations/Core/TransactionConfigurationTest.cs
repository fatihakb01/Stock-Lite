using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class TransactionConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly TransactionHelper _transactionHelper;
    private readonly ProductHelper _productHelper;

    public TransactionConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _transactionHelper = new TransactionHelper(fixture);
        _productHelper = new ProductHelper(fixture);
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenTransactionIdIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            transactionId: null!,
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenTransactionIdExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            transactionId: new string('A', 51),
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenTransactionIdIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            transactionId: new string('A', 50));

        Assert.True(await Fixture.Context.Transactions
            .AnyAsync(x => x.Id == transaction.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenPaymentMethodExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            saveChanges: false);

        transaction.PaymentMethod = new string('A', 51);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenPaymentMethodIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            saveChanges: false);

        transaction.PaymentMethod = new string('A', 50);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Transactions
            .AnyAsync(x => x.Id == transaction.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenNotesExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            saveChanges: false);

        transaction.Notes = new string('A', 201);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenNotesIsExactlyMaxLength_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date,
            saveChanges: false);

        transaction.Notes = new string('A', 200);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Transactions
            .AnyAsync(x => x.Id == transaction.Id));
    }

    // -------------------------------------------------------
    // Check constraint
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenTransactionDateIsInFuture_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.AddDays(1),
            saveChanges: false);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenTransactionDateIsNow_Succeeds()
    {
        var company = await _companyHelper.CreateCompany();

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            transactionDate: DateTime.UtcNow.Date);

        Assert.True(await Fixture.Context.Transactions
            .AnyAsync(x => x.Id == transaction.Id));
    }

    // -------------------------------------------------------
    // Unique index
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateTransactionIdWithinCompany_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.Date,
            transactionId: "TR-0001");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            _transactionHelper.CreateTransaction(
                company,
                DateTime.UtcNow.Date,
                transactionId: "TR-0001"));
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateTransactionIdAcrossCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company1,
            DateTime.UtcNow.Date,
            transactionId: "TR-0001");

        await _transactionHelper.CreateTransaction(
            company2,
            DateTime.UtcNow.Date,
            transactionId: "TR-0001");

        var count = await Fixture.Context.Transactions
            .CountAsync(x => x.TransactionId == "TR-0001");

        Assert.Equal(2, count);
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteTransaction_DeletesAssociatedLineItems()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var transaction = await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.AddDays(-1));

        await _transactionHelper.CreateTransactionLineItem(
            transaction,
            product,
            quantity: 10,
            unitPrice: 5);

        Fixture.Context.Transactions.Remove(transaction);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.TransactionLineItems
            .AnyAsync(x =>
                x.TransactionId == transaction.Id &&
                x.ProductId == product.Id));
    }

    // -------------------------------------------------------
    // Restrict delete
    // -------------------------------------------------------
    // Similar coverage already exists in CompanyConfigurationTest:
    // DeleteCompany_WhenTransactionsExist_ThrowsException

    [Fact]
    public async Task DeleteCompany_WhenTransactionsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _transactionHelper.CreateTransaction(
            company,
            DateTime.UtcNow.AddDays(-1));

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));
    }
}