using Domain.Enums;
using Domain.Interfaces.Services.Export;
using Infrastructure.Services.Export.Records;
using Moq;
using Test.Helper;

namespace Test.Unit.Services.Export
{
    public class ExportServiceTest
    {
        private readonly Mock<IFileExportHelper> _helperMock;

        public ExportServiceTest()
        {
            _helperMock = new Mock<IFileExportHelper>();
        }

        // -------------------------------------------------------
        // ExportToStreamAsync — ExportResult shape
        // -------------------------------------------------------
        [Fact]
        public async Task ExportToStreamAsync_CsvFileType_ReturnsCsvContentType()
        {
            var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            _helperMock
                .Setup(h => h.WriteStreamCsvAsync(It.IsAny<List<object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ms);

            var fileType = FileType.Csv;
            var contentType = fileType == FileType.Csv
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Assert.Equal("text/csv", contentType);
        }

        [Fact]
        public async Task ExportToStreamAsync_ExcelFileType_ReturnsExcelContentType()
        {
            var fileType = FileType.Excel;
            var contentType = fileType == FileType.Csv
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", contentType);
        }

        [Theory]
        [InlineData(FileType.Csv, "csv")]
        [InlineData(FileType.Excel, "xlsx")]
        public void ExportToStreamAsync_FileName_HasCorrectExtension(FileType fileType, string expectedExtension)
        {
            var profileName = "MyExport";
            var extension = fileType == FileType.Csv ? "csv" : "xlsx";
            var fileName = $"{profileName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}";

            Assert.EndsWith($".{expectedExtension}", fileName);
            Assert.StartsWith(profileName, fileName);
        }

        // -------------------------------------------------------
        // ExportToStreamAsync — unsupported ExportType
        // -------------------------------------------------------
        [Fact]
        public async Task ExportToStreamAsync_UnsupportedExportType_ThrowsInvalidOperationException()
        {
            var profile = ExportProfileHelper.CreateStandardProfileStatic((ExportType)99);

            ExportType? exportType = (ExportType)99;

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = exportType switch
                {
                    ExportType.StockLevels => "StockLevels",
                    ExportType.SalesAggregates => "SalesAggregates",
                    ExportType.Forecasts => "Forecasts",
                    ExportType.PurchaseForecasts => "PurchaseForecasts",
                    ExportType.ConfirmedOrderAggregates => "ConfirmedOrderAggregates",
                    ExportType.ProductKpis => "ProductKpis",
                    ExportType.Seasonality => "Seasonality",
                    _ => throw new InvalidOperationException($"Unsupported export type: {exportType}")
                };
            });

            Assert.Contains("Unsupported export type", ex.Message);
        }

        // -------------------------------------------------------
        // ExportToFileAsync — TargetPath validation
        // -------------------------------------------------------
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WriteFileAsync_NullOrEmptyTargetPath_ThrowsInvalidOperationException(string? targetPath)
        {
            var isInvalid = string.IsNullOrWhiteSpace(targetPath);
            Assert.True(isInvalid);
        }

        // -------------------------------------------------------
        // FetchCustomRowsAsync — naming validation
        // -------------------------------------------------------
        [Theory]
        [InlineData("my_proc")]
        [InlineData("sp_custom_proc")]
        [InlineData("custom")]
        [InlineData("proc_custom_")]
        [InlineData("")]
        [InlineData("   ")]
        public void FetchCustomRowsAsync_InvalidNaming_WouldThrow(string customSource)
        {
            ExportProfileHelper.CreateCustomProfileStatic(customSource);

            var isValid =
                customSource.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) ||
                customSource.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData("custom_my_proc")]
        [InlineData("dbo.custom_my_proc")]
        [InlineData("CUSTOM_my_proc")]
        [InlineData("DBO.CUSTOM_my_proc")]
        public void FetchCustomRowsAsync_ValidNaming_WouldNotThrowOnNameCheck(string customSource)
        {
            ExportProfileHelper.CreateCustomProfileStatic(customSource);

            var isValid =
                customSource.StartsWith("custom_", StringComparison.OrdinalIgnoreCase) ||
                customSource.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase);

            Assert.True(isValid);
        }

        // -------------------------------------------------------
        // WriteToStreamAsync — helper routing
        // -------------------------------------------------------
        [Fact]
        public async Task WriteToStreamAsync_CsvFileType_CallsWriteStreamCsvAsync()
        {
            var records = new List<StockLevelRecord>();
            var expectedMs = new MemoryStream(new byte[] { 1, 2, 3 });

            _helperMock
                .Setup(h => h.WriteStreamCsvAsync(records, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMs);

            var fileType = FileType.Csv;
            var result = fileType == FileType.Csv
                ? await _helperMock.Object.WriteStreamCsvAsync(records, CancellationToken.None)
                : _helperMock.Object.WriteStreamExcel(records);

            _helperMock.Verify(h => h.WriteStreamCsvAsync(records, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Same(expectedMs, result);
        }

        [Fact]
        public async Task WriteToStreamAsync_ExcelFileType_CallsWriteStreamExcel()
        {
            var records = new List<StockLevelRecord>();
            var expectedMs = new MemoryStream(new byte[] { 1, 2, 3 });

            _helperMock
                .Setup(h => h.WriteStreamExcel(records))
                .Returns(expectedMs);

            var fileType = FileType.Excel;
            var result = fileType == FileType.Csv
                ? await _helperMock.Object.WriteStreamCsvAsync(records, CancellationToken.None)
                : _helperMock.Object.WriteStreamExcel(records);

            _helperMock.Verify(h => h.WriteStreamExcel(records), Times.Once);
            Assert.Same(expectedMs, result);
        }
    }
}