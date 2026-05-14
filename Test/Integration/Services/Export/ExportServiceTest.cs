using Domain.Enums;
using Domain.Interfaces.Services.Export;
using Infrastructure.Services.Export;
using Infrastructure.Services.Export.Records;
using Moq;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Services.Export;

public class ExportServiceTest(DatabaseFixture fixture)
    : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
{
    private readonly Mock<IFileExportHelper> _helperMock = new();

    private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
    private readonly ProductHelper _productHelper = new ProductHelper(fixture);
    private readonly ForecastHelper _forecastHelper = new ForecastHelper(fixture);
    private readonly ExportProfileHelper _exportProfileHelper = new ExportProfileHelper(fixture);

    private ExportService CreateService()
        => new(Fixture.Context, _helperMock.Object);

    // -------------------------------------------------------
    // ExportToStreamAsync
    // -------------------------------------------------------
    [Theory]
    [InlineData(ExportType.StockLevels)]
    [InlineData(ExportType.SalesAggregates)]
    [InlineData(ExportType.Forecasts)]
    [InlineData(ExportType.PurchaseForecasts)]
    [InlineData(ExportType.ConfirmedOrderAggregates)]
    [InlineData(ExportType.ProductKpis)]
    [InlineData(ExportType.Seasonality)]
    public async Task ExportToStreamAsync_WhenCsv_ReturnsExportResult(
        ExportType exportType)
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: exportType,
            company: company);

        var expectedStream = new MemoryStream([1, 2, 3]);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<StockLevelRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<SalesAggregateRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<ForecastRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<PurchaseForecastRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<ConfirmedOrderAggregateRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<ProductKpiRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<SeasonalityRecord>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStream);

        var result = await service.ExportToStreamAsync(
            profile,
            FileType.Csv,
            company.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("text/csv", result.ContentType);
        Assert.EndsWith(".csv", result.FileName);
        Assert.NotEmpty(result.Content);
    }

    [Theory]
    [InlineData(ExportType.StockLevels)]
    [InlineData(ExportType.SalesAggregates)]
    [InlineData(ExportType.Forecasts)]
    [InlineData(ExportType.PurchaseForecasts)]
    [InlineData(ExportType.ConfirmedOrderAggregates)]
    [InlineData(ExportType.ProductKpis)]
    [InlineData(ExportType.Seasonality)]
    public async Task ExportToStreamAsync_WhenExcel_ReturnsExportResult(
        ExportType exportType)
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: exportType,
            company: company);

        var expectedStream = new MemoryStream([1, 2, 3]);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<StockLevelRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<SalesAggregateRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<ForecastRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<PurchaseForecastRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<ConfirmedOrderAggregateRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<ProductKpiRecord>>()))
            .Returns(expectedStream);

        _helperMock
            .Setup(h => h.WriteStreamExcel(It.IsAny<List<SeasonalityRecord>>()))
            .Returns(expectedStream);

        var result = await service.ExportToStreamAsync(
            profile,
            FileType.Excel,
            company.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.ContentType);

        Assert.EndsWith(".xlsx", result.FileName);
        Assert.NotEmpty(result.Content);
    }

    // -------------------------------------------------------
    // ExportToFileAsync
    // -------------------------------------------------------
    [Fact]
    public async Task ExportToFileAsync_WhenCsv_CallsWriteCsvAsync()
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: ExportType.StockLevels,
            targetPath: Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv"),
            company: company);

        _helperMock
            .Setup(h => h.GetFileTypeFromPath(profile.TargetPath!))
            .Returns(FileType.Csv);

        _helperMock
            .Setup(h => h.WriteCsvAsync(
                It.IsAny<List<StockLevelRecord>>(),
                profile.TargetPath!,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await service.ExportToFileAsync(
            profile,
            company.Id,
            CancellationToken.None);

        _helperMock.Verify(h =>
            h.WriteCsvAsync(
                It.IsAny<List<StockLevelRecord>>(),
                profile.TargetPath!,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExportToFileAsync_WhenExcel_CallsWriteExcel()
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: ExportType.StockLevels,
            targetPath: Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv"),
            company: company);

        _helperMock
            .Setup(h => h.GetFileTypeFromPath(profile.TargetPath!))
            .Returns(FileType.Excel);

        await service.ExportToFileAsync(
            profile,
            company.Id,
            CancellationToken.None);

        _helperMock.Verify(h =>
            h.WriteExcel(
                It.IsAny<List<StockLevelRecord>>(),
                profile.TargetPath!),
            Times.Once);
    }

    // -------------------------------------------------------
    // Company filtering
    // -------------------------------------------------------
    [Fact]
    public async Task ExportToStreamAsync_WhenCompanySpecified_OnlyExportsThatCompany()
    {
        var service = CreateService();
        var company1 = await _companyHelper.CreateCompany("Company 1");
        var company2 = await _companyHelper.CreateCompany("Company 2");

        var product1 = await _productHelper.CreateProduct(
            company1,
            sku: "SKU-0001");

        var product2 = await _productHelper.CreateProduct(
            company2,
            sku: "SKU-0002");

        await _forecastHelper.CreateForecast(product1, Period.Monthly);
        await _forecastHelper.CreateForecast(product2, Period.Monthly);

        List<ForecastRecord>? captured = null;

        _helperMock
            .Setup(h => h.WriteStreamCsvAsync(
                It.IsAny<List<ForecastRecord>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<ForecastRecord>, CancellationToken>((r, _) =>
            {
                captured = r;
            })
            .ReturnsAsync(new MemoryStream([1, 2, 3]));

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: ExportType.Forecasts,
            company: company1);

        await service.ExportToStreamAsync(
            profile,
            FileType.Csv,
            company1.Id,
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Single(captured!);
        Assert.Equal("SKU-0001", captured[0].SKU);
    }

    // -------------------------------------------------------
    // Custom exports
    // -------------------------------------------------------
    [Fact]
    public async Task ExportToStreamAsync_WhenInvalidCustomSourcePrefix_Throws()
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateCustomProfile(
            customSource: "invalid_proc",
            company: company);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportToStreamAsync(
                profile,
                FileType.Csv,
                company.Id,
                CancellationToken.None));

        Assert.Contains("Invalid custom source name", ex.Message);
    }

    [Fact]
    public async Task ExportToStreamAsync_WhenCustomSourceDoesNotExist_Throws()
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateCustomProfile(
            customSource: "dbo.custom_does_not_exist",
            company: company);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportToStreamAsync(
                profile,
                FileType.Csv,
                company.Id,
                CancellationToken.None));

        Assert.Contains("does not exist", ex.Message);
    }

    // -------------------------------------------------------
    // Unsupported export type
    // -------------------------------------------------------
    [Fact]
    public async Task ExportToStreamAsync_WhenUnsupportedExportType_Throws()
    {
        var service = CreateService();
        var company = await _companyHelper.CreateCompany();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: (ExportType)999,
            company: company);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExportToStreamAsync(
                profile,
                FileType.Csv,
                company.Id,
                CancellationToken.None));

        Assert.Contains("Unsupported export type", ex.Message);
    }

    // -------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------
    [Fact]
    public async Task ExportToStreamAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var service = CreateService();

        var company = await _companyHelper.CreateCompany();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var profile = await _exportProfileHelper.CreateStandardProfile(
            exportType: ExportType.StockLevels,
            company: company);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExportToStreamAsync(
                profile,
                FileType.Csv,
                company.Id,
                cts.Token));
    }
}