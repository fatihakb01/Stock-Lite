using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Derived;

public class ProductKpiConfigurationTest
    : IntegrationTestBase, IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper;
    private readonly ProductKpiHelper _productKpiHelper;

    public ProductKpiConfigurationTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _companyHelper = new CompanyHelper(fixture);
        _productKpiHelper = new ProductKpiHelper(fixture);
    }

    // -------------------------------------------------------
    // Primary key uniqueness
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenDuplicateCompanyId_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();

        await _productKpiHelper.CreateProductKpi(company);

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            _productKpiHelper.CreateProductKpi(company));
    }

    [Fact]
    public async Task SaveChanges_WhenDifferentCompanies_Succeeds()
    {
        var company1 = await _companyHelper.CreateCompany();
        var company2 = await _companyHelper.CreateCompany();

        await _productKpiHelper.CreateProductKpi(company1);
        await _productKpiHelper.CreateProductKpi(company2);

        Assert.Equal(
            2,
            await Fixture.Context.ProductKpis.CountAsync());
    }
}