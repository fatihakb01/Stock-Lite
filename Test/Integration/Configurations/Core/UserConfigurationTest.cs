using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Resend;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class UserConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly UserHelper _userHelper;
    private readonly Mock<IResend> _resendMock;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public UserConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);

        _resendMock = new Mock<IResend>();
        _envMock = new Mock<IWebHostEnvironment>();
        _userHelper = new UserHelper(fixture, _resendMock, _envMock);
    }

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenUserNameIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.UserName = null!;

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenEmailIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.Email = null!;

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenPasswordHashIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.PasswordHash = null!;

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Max length constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenUserNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.UserName = new string('A', 51);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenUserNameIsMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.UserName = new string('A', 50);
        user.PasswordHash = "hash";

        Fixture.Context.Users.Add(user);
        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Users.AnyAsync(x => x.Id == user.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenFirstNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.FirstName = new string('A', 51);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenFirstNameIsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.FirstName = new string('A', 50);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenLastNameExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.LastName = new string('A', 51);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenLastNameIsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.LastName = new string('A', 50);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenEmailExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.Email = new string('A', 256);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenEmailIsMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.Email = new string('A', 255);
        user.PasswordHash = "hash";

        Fixture.Context.Users.Add(user);
        Fixture.Context.SaveChanges();

        Assert.True(await Fixture.Context.Users.AnyAsync(x => x.Id == user.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenPhoneNumberExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.PhoneNumber = new string('1', 21);

        Fixture.Context.Users.Add(user);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenPhoneNumberIsMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var user = UserHelper.CreateUser(company);
        user.Email = new string('1', 20);
        user.PasswordHash = "hash";

        Fixture.Context.Users.Add(user);
        Fixture.Context.SaveChanges();

        Assert.True(await Fixture.Context.Users.AnyAsync(x => x.Id == user.Id));
    }

    // -------------------------------------------------------
    // Required FK behavior
    // -------------------------------------------------------
    // see DeleteCompany_DeletesAssociatedUsers inside CompanyConfigurationTest.cs

    // -------------------------------------------------------
    // Unique indexes
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenEmailIsDuplicate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _userHelper.CreateDummyUserAsync(company, email: "test@test.com");

        var duplicate = UserHelper.CreateUser(company, email: "test@test.com");
        duplicate.PasswordHash = "hash2";

        Fixture.Context.Users.Add(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenUserNamePerCompanyIsDuplicate_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _userHelper.CreateDummyUserAsync(company, userName: "same-user");

        var duplicate = UserHelper.CreateUser(company, userName: "same-user");
        duplicate.Email = "other@test.com";
        duplicate.PasswordHash = "hash2";

        Fixture.Context.Users.Add(duplicate);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenSameUserNameDifferentCompany_AllowsInsert()
    {
        var company1 = await _companyHelper.CreateCompany(name: "C1", saveChanges: false);
        var company2 = await _companyHelper.CreateCompany(name: "C2", saveChanges: false);

        await _userHelper.CreateDummyUserAsync(company1, userName: "same-user");

        var user2 = UserHelper.CreateUser(company2, userName: "same-user");
        user2.Email = "other@test.com";
        user2.PasswordHash = "hash2";

        Fixture.Context.Users.Add(user2);
        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.Users.AnyAsync(x => x.Id == user2.Id));
    }
}