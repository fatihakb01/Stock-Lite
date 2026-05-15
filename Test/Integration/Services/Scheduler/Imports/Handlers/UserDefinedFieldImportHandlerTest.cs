using Infrastructure.Services.Scheduler.Imports.Handlers;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Scheduler.Imports.Handlers;

public class UserDefinedFieldImportHandlerTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly CompanyHelper _companyHelper = new(fixture);
    private readonly ProductHelper _productHelper = new(fixture);
    private readonly UserDefinedFieldHelper _udfHelper = new(fixture);

    private UserDefinedFieldImportHandler CreateRepository()
        => new(Fixture.Context);

    // -------------------------------------------------------
    // ImportAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ImportAsync_WhenValidCsv_ImportsUserDefinedField()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001");

        var handler = CreateRepository();

        var csv = UserDefinedFieldHelper.BuildCsv(
            sku: "SKU-0001",
            key: "Color",
            value: "Red");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(stream, company.Id, CancellationToken.None);

        var count = await _udfHelper.CountUserDefinedFields(company.Id);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ImportAsync_WhenProductDoesNotExist_DoesNotInsertField()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = UserDefinedFieldHelper.BuildCsv(
            sku: "UNKNOWN-SKU",
            key: "Color",
            value: "Red");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(stream, company.Id, CancellationToken.None);

        var count = await _udfHelper.CountUserDefinedFields(company.Id);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ImportAsync_WhenFieldAlreadyExists_UpdatesField()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001");

        var handler = CreateRepository();

        // initial import
        var csv1 = UserDefinedFieldHelper.BuildCsv(
            sku: "SKU-0001",
            key: "Color",
            value: "Red");

        await using (var stream1 = FileStreamHelper.CreateCsvStream(csv1))
        {
            await handler.ImportAsync(stream1, company.Id, CancellationToken.None);
        }

        // update import
        var csv2 = UserDefinedFieldHelper.BuildCsv(
            sku: "SKU-0001",
            key: "Color",
            value: "Blue");

        await using var stream2 = FileStreamHelper.CreateCsvStream(csv2);

        await handler.ImportAsync(stream2, company.Id, CancellationToken.None);

        var count = await _udfHelper.CountUserDefinedFields(company.Id);

        Assert.Equal(1, count);

        var field = await Fixture.Context.UserDefinedFields
            .SingleAsync(x =>
                x.CompanyId == company.Id &&
                x.Key == "Color");

        Assert.Equal("Blue", field.Value);
    }

    [Fact]
    public async Task ImportAsync_WhenValueIsNull_ImportsAsEmptyString()
    {
        var company = await _companyHelper.CreateCompany();

        await _productHelper.CreateProduct(
            company: company,
            sku: "SKU-0001");

        var handler = CreateRepository();

        var csv = UserDefinedFieldHelper.BuildCsv(
            sku: "SKU-0001",
            key: "Color",
            value: "");

        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(stream, company.Id, CancellationToken.None);

        var field = await Fixture.Context.UserDefinedFields
            .SingleAsync(x =>
                x.CompanyId == company.Id &&
                x.Key == "Color");

        Assert.Equal(string.Empty, field.Value);
    }

    [Fact]
    public async Task ImportAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = UserDefinedFieldHelper.BuildCsv();
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.ImportAsync(stream, company.Id, cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WhenCsvIsInvalid_ThrowsException()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var invalidCsv =
            "InvalidHeader\n" +
            "InvalidValue";

        await using var stream = FileStreamHelper.CreateCsvStream(invalidCsv);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            handler.ImportAsync(stream, company.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ImportAsync_CleansUpStagingTable()
    {
        var company = await _companyHelper.CreateCompany();
        var handler = CreateRepository();

        var csv = UserDefinedFieldHelper.BuildCsv();
        await using var stream = FileStreamHelper.CreateCsvStream(csv);

        await handler.ImportAsync(stream, company.Id, CancellationToken.None);

        var remainingRows = await Fixture.Context.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS Value
                FROM dbo.StagingUserDefinedFields
                WHERE CompanyId = {0}
                """,
                company.Id)
            .SingleAsync();

        Assert.Equal(0, remainingRows);
    }
}