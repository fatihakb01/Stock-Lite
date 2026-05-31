using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class ExportProfileConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenNameIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company);

        profile.Name = null!;

        Fixture.Context.ExportProfile.Add(profile);

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

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company,
            name: new string('A', 51));

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenNameIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company,
            name: new string('A', 50));

        Fixture.Context.ExportProfile.Add(profile);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.ExportProfile
            .AnyAsync(x => x.Id == profile.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company);

        profile.Description = new string('A', 201);

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenDescriptionIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company);

        profile.Description = new string('A', 200);

        Fixture.Context.ExportProfile.Add(profile);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.ExportProfile
            .AnyAsync(x => x.Id == profile.Id));
    }

    [Fact]
    public async Task SaveChanges_WhenCustomSourceExceedsMaxLength_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateCustomProfileStatic(
            company: company,
            customSource: new string('A', 101));

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenCustomSourceIsExactlyMaxLength_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateCustomProfileStatic(
            company: company,
            customSource: new string('A', 100));

        Fixture.Context.ExportProfile.Add(profile);

        await Fixture.Context.SaveChangesAsync();

        Assert.True(await Fixture.Context.ExportProfile
            .AnyAsync(x => x.Id == profile.Id));
    }

    // -------------------------------------------------------
    // Check constraints
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenExportTypeAndCustomSourceAreBothSet_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company, exportType: ExportType.SalesAggregates, 
            customSource: "dbo.custom_export", type: CustomType.Procedure);

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenExportTypeAndCustomSourceAreBothNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company, customSource: null);

        profile.ExportType = null;

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenCustomSourceExistsButCustomTypeIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateCustomProfileStatic(
            company: company, customSource: "dbo.custom_export");

        profile.CustomType = null;

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenCustomSourceIsNullButCustomTypeExists_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        var profile = ExportProfileHelper.CreateStandardProfileStatic(
            company: company, customSource: null);

        profile.ExportType = null;
        profile.CustomType = CustomType.View;

        Fixture.Context.ExportProfile.Add(profile);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            Fixture.Context.SaveChangesAsync());
    }

    // -------------------------------------------------------
    // Cascade delete
    // -------------------------------------------------------
    // see DeleteCompany_DeletesAssociatedExportProfiles inside CompanyConfigurationTest.cs
}