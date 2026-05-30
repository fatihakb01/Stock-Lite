using Domain.Entities.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Resend;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class CompanyConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly Mock<IResend> _resendMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly UserHelper _userHelper;
    private readonly ProductHelper _productHelper;
    private readonly TransactionHelper _transactionHelper;
    private readonly PurchaseOrderHelper _purchaseOrderHelper;
    private readonly ConfirmedOrderHelper _confirmedOrderHelper;
    private readonly SchedulerHelper _schedulerHelper;
    private readonly ExportProfileHelper _exportProfileHelper;
    private readonly ProductKpiHelper _productKpiHelper;

    public CompanyConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _resendMock = new Mock<IResend>();
        _envMock = new Mock<IWebHostEnvironment>();
        _productHelper = new ProductHelper(fixture);
        _transactionHelper = new TransactionHelper(fixture);
        _purchaseOrderHelper = new PurchaseOrderHelper(fixture);
        _confirmedOrderHelper = new ConfirmedOrderHelper(fixture);
        _schedulerHelper = new SchedulerHelper(fixture);
        _exportProfileHelper = new ExportProfileHelper(fixture);
        _productKpiHelper = new ProductKpiHelper(fixture);

        _userHelper = new UserHelper(
            fixture,
            _resendMock,
            _envMock
        );
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameIsNull_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.Name = null!;

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenEmailIsNull_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.Email = null!;

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameExceedsMaxLength_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic(
            name: new string('A', 101));

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = CompanyHelper.CreateCompanyStatic(
            name: new string('A', 100));

        Fixture.Context.Companies.Add(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Companies
            .AnyAsync(x => x.Id == company.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenEmailExceedsMaxLength_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic(
            email: new string('A', 256));

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenEmailIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = CompanyHelper.CreateCompanyStatic(
            email: new string('A', 255));

        Fixture.Context.Companies.Add(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Companies
            .AnyAsync(x => x.Id == company.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenPhoneNumberExceedsMaxLength_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.PhoneNumber = new string('1', 21);

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenPhoneNumberIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.PhoneNumber = new string('1', 20);

        Fixture.Context.Companies.Add(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Companies
            .AnyAsync(x => x.Id == company.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenAddressExceedsMaxLength_ThrowsException()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.Address = new string('A', 251);

        Fixture.Context.Companies.Add(company);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenAddressIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = CompanyHelper.CreateCompanyStatic();
        company.Address = new string('A', 250);

        Fixture.Context.Companies.Add(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Companies
            .AnyAsync(x => x.Id == company.Id));
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteCompany_DeletesAssociatedUsers()
    {
        var company = await _companyHelper.CreateCompany();
        var user = await _userHelper.CreateDummyUserAsync(company);

        Fixture.Context.Companies.Remove(company);
        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.Users
            .AnyAsync(x => x.Id == user.Id));
    }

    [Fact]
    public async Task DeleteCompany_DeletesAssociatedJobTasks()
    {
        var company = await _companyHelper.CreateCompany();
        var task = await _schedulerHelper.CreateJobTask(companyId: company.Id);

        Fixture.Context.Companies.Remove(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.JobTasks
            .AnyAsync(x => x.Id == task.Id));
    }

    [Fact]
    public async Task DeleteCompany_DeletesAssociatedExportProfiles()
    {
        var company = await _companyHelper.CreateCompany();
        var profile = await _exportProfileHelper.CreateStandardProfile(company: company);

        Fixture.Context.Companies.Remove(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.ExportProfile
            .AnyAsync(x => x.Id == profile.Id));
    }

    [Fact]
    public async Task DeleteCompany_DeletesAssociatedProductKpi()
    {
        var company = await _companyHelper.CreateCompany();
        var kpi = await _productKpiHelper.CreateProductKpi(company: company);

        Fixture.Context.Companies.Remove(company);

        await Fixture.Context.SaveChangesAsync();

        Assert.False(await Fixture.Context.ProductKpis
            .AnyAsync(x => x.CompanyId == company.Id));
    }

    // -------------------------------------------------------
    // Restrict delete
    // -------------------------------------------------------
    [Fact]
    public async Task DeleteCompany_WhenProductsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(company);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task DeleteCompany_WhenTransactionsExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        DateTime transactionDate = DateTime.UtcNow.Date.AddDays(-2);

        await _transactionHelper.CreateTransaction(company: company, transactionDate: transactionDate);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task DeleteCompany_WhenPurchaseOrdersExist_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _purchaseOrderHelper.CreatePurchaseOrder(company);

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.Companies.Remove(company));

        Fixture.Context.ChangeTracker.Clear();
    }

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