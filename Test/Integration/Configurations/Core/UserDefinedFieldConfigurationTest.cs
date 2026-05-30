using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Configurations.Core;

public class UserDefinedFieldConfigurationTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly UserDefinedFieldHelper _udfHelper = new UserDefinedFieldHelper(fixture);

    // -------------------------------------------------------
    // Composite Key behavior
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_AllowsUniqueCompositeKey()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _udfHelper.CreateUserDefinedField(product, key: "Color", value: "Red");

        var exists = await Fixture.Context.UserDefinedFields.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.CompanyId == company.Id &&
            x.Key == "Color");

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveChanges_WhenDuplicateCompositeKey_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _udfHelper.CreateUserDefinedField(product, key: "Size", value: "L");

        // second insert with same composite key
        var duplicate = new UserDefinedField
        {
            ProductId = product.Id,
            CompanyId = company.Id,
            Product = product,
            Key = "Size",
            Value = "XL"
        };

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.UserDefinedFields.Add(duplicate));

        Fixture.Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------
    // Required relationship behavior
    // -------------------------------------------------------
    // see DeleteProduct_DeletesAssociatedUserDefinedFields inside ProductConfigurationTest.cs

    // -------------------------------------------------------
    // Required fields
    // -------------------------------------------------------
    [Fact]
    public async Task SaveChanges_WhenKeyIsNull_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        var field = new UserDefinedField
        {
            ProductId = product.Id,
            CompanyId = company.Id,
            Product = product,
            Key = null!
        };

        Assert.ThrowsAny<InvalidOperationException>(() =>
            Fixture.Context.UserDefinedFields.Add(field));

        Fixture.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task SaveChanges_WhenValueIsNull_SavesSuccessfully()
    {
        var company = await _companyHelper.CreateCompany();
        var product = await _productHelper.CreateProduct(company);

        await _udfHelper.CreateUserDefinedField(product, key: "Batch", value: null);

        var exists = await Fixture.Context.UserDefinedFields.AnyAsync(x =>
            x.ProductId == product.Id &&
            x.Key == "Batch");

        Assert.True(exists);
    }
}