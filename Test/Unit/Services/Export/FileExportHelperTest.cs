using ClosedXML.Excel;
using Domain.Enums;
using Infrastructure.Services.Export;

namespace Test.Unit.Services.Export
{
    public class FileExportHelperTest
    {
        private readonly FileExportHelper _helper = new FileExportHelper();

        // --- Test record used for typed export methods ---
        private sealed class TestRecord
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        private static List<TestRecord> SomeRecords() =>
        [
            new TestRecord { Name = "Alpha", Value = 1 },
            new TestRecord { Name = "Beta",  Value = 2 }
        ];

        private static List<Dictionary<string, object?>> SomeDictionaryRows() =>
        [
            new Dictionary<string, object?> { ["SKU"] = "A001", ["Qty"] = 10 },
            new Dictionary<string, object?> { ["SKU"] = "B002", ["Qty"] = 20 }
        ];

        // -------------------------------------------------------
        // GetFileTypeFromPath
        // -------------------------------------------------------
        [Theory]
        [InlineData("C:\\Exports\\file.csv")]
        [InlineData("C:\\Exports\\file.CSV")]
        [InlineData("/exports/file.csv")]
        public void GetFileTypeFromPath_CsvPath_ReturnsCsv(string path)
        {
            var result = _helper.GetFileTypeFromPath(path);
            Assert.Equal(FileType.Csv, result);
        }

        [Theory]
        [InlineData("C:\\Exports\\file.xlsx")]
        [InlineData("C:\\Exports\\file.XLSX")]
        [InlineData("/exports/file.xlsx")]
        public void GetFileTypeFromPath_ExcelPath_ReturnsExcel(string path)
        {
            var result = _helper.GetFileTypeFromPath(path);
            Assert.Equal(FileType.Excel, result);
        }

        [Theory]
        [InlineData("C:\\Exports\\file.txt")]
        [InlineData("C:\\Exports\\file.json")]
        [InlineData("C:\\Exports\\file")]
        [InlineData("")]
        public void GetFileTypeFromPath_UnsupportedExtension_ThrowsInvalidOperationException(string path)
        {
            Assert.Throws<InvalidOperationException>(() =>
                _helper.GetFileTypeFromPath(path));
        }

        [Fact]
        public void GetFileTypeFromPath_NullPath_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _helper.GetFileTypeFromPath(null!));
        }

        // -------------------------------------------------------
        // WriteStreamCsvAsync
        // -------------------------------------------------------
        [Fact]
        public async Task WriteStreamCsvAsync_WithRecords_ReturnsNonEmptyStream()
        {
            var ms = await _helper.WriteStreamCsvAsync(SomeRecords(), CancellationToken.None);

            Assert.NotNull(ms);
            Assert.True(ms.Length > 0);
            Assert.Equal(0, ms.Position);
        }

        [Fact]
        public async Task WriteStreamCsvAsync_WithRecords_StreamContainsHeaders()
        {
            var ms = await _helper.WriteStreamCsvAsync(SomeRecords(), CancellationToken.None);

            ms.Position = 0;
            var content = new StreamReader(ms).ReadToEnd();

            Assert.Contains("Name", content);
            Assert.Contains("Value", content);
        }

        [Fact]
        public async Task WriteStreamCsvAsync_WithRecords_StreamContainsData()
        {
            var ms = await _helper.WriteStreamCsvAsync(SomeRecords(), CancellationToken.None);

            ms.Position = 0;
            var content = new StreamReader(ms).ReadToEnd();

            Assert.Contains("Alpha", content);
            Assert.Contains("Beta", content);
        }

        [Fact]
        public async Task WriteStreamCsvAsync_EmptyList_ReturnsStreamWithHeaderOnly()
        {
            var ms = await _helper.WriteStreamCsvAsync(new List<TestRecord>(), CancellationToken.None);

            ms.Position = 0;
            var content = new StreamReader(ms).ReadToEnd();

            // CsvHelper still writes headers for empty lists
            Assert.Contains("Name", content);
            Assert.True(ms.Length > 0);
        }

        // -------------------------------------------------------
        // WriteCsvAsync (file path)
        // -------------------------------------------------------
        [Fact]
        public async Task WriteCsvAsync_ValidPath_WritesFile()
        {
            var path = Path.GetTempFileName();
            path = Path.ChangeExtension(path, ".csv");

            try
            {
                await _helper.WriteCsvAsync(SomeRecords(), path, CancellationToken.None);

                Assert.True(File.Exists(path));
                var content = await File.ReadAllTextAsync(path);
                Assert.Contains("Alpha", content);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task WriteCsvAsync_InvalidPath_Throws()
        {
            var invalidPath = "Z:\\NonExistentDrive\\file.csv";

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _helper.WriteCsvAsync(SomeRecords(), invalidPath, CancellationToken.None));
        }

        // -------------------------------------------------------
        // WriteStreamExcel
        // -------------------------------------------------------
        [Fact]
        public void WriteStreamExcel_WithRecords_ReturnsNonEmptyStream()
        {
            var ms = _helper.WriteStreamExcel(SomeRecords());

            Assert.NotNull(ms);
            Assert.True(ms.Length > 0);
            Assert.Equal(0, ms.Position);
        }

        [Fact]
        public void WriteStreamExcel_WithRecords_StreamContainsCorrectData()
        {
            var ms = _helper.WriteStreamExcel(SomeRecords());

            ms.Position = 0;
            using var workbook = new XLWorkbook(ms);
            var sheet = workbook.Worksheet("Export");

            Assert.Equal("Name", sheet.Cell(1, 1).Value.ToString());
            Assert.Equal("Value", sheet.Cell(1, 2).Value.ToString());
            Assert.Equal("Alpha", sheet.Cell(2, 1).Value.ToString());
            Assert.Equal("Beta", sheet.Cell(3, 1).Value.ToString());
        }

        [Fact]
        public void WriteStreamExcel_EmptyList_ReturnsStreamWithHeadersOnly()
        {
            var ms = _helper.WriteStreamExcel(new List<TestRecord>());

            ms.Position = 0;
            using var workbook = new XLWorkbook(ms);
            var sheet = workbook.Worksheet("Export");

            Assert.Equal("Name", sheet.Cell(1, 1).Value.ToString());
            Assert.Equal("Value", sheet.Cell(1, 2).Value.ToString());
            Assert.Equal("", sheet.Cell(2, 1).Value.ToString());
        }

        // -------------------------------------------------------
        // WriteExcel (file path)
        // -------------------------------------------------------
        [Fact]
        public void WriteExcel_ValidPath_WritesFile()
        {
            var path = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");

            try
            {
                _helper.WriteExcel(SomeRecords(), path);

                Assert.True(File.Exists(path));
                Assert.True(new FileInfo(path).Length > 0);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WriteExcel_InvalidPath_Throws()
        {
            var invalidPath = "Z:\\NonExistentDrive\\file.xlsx";

            Assert.ThrowsAny<Exception>(() =>
                _helper.WriteExcel(SomeRecords(), invalidPath));
        }

        // -------------------------------------------------------
        // WriteStreamDictionaryCsvAsync
        // -------------------------------------------------------
        [Fact]
        public async Task WriteStreamDictionaryCsvAsync_WithRows_ReturnsNonEmptyStream()
        {
            var ms = await _helper.WriteStreamDictionaryCsvAsync(SomeDictionaryRows());

            Assert.NotNull(ms);
            Assert.True(ms.Length > 0);
            Assert.Equal(0, ms.Position);
        }

        [Fact]
        public async Task WriteStreamDictionaryCsvAsync_WithRows_ContainsHeadersAndData()
        {
            var ms = await _helper.WriteStreamDictionaryCsvAsync(SomeDictionaryRows());

            ms.Position = 0;
            var content = new StreamReader(ms).ReadToEnd();

            Assert.Contains("SKU", content);
            Assert.Contains("Qty", content);
            Assert.Contains("A001", content);
            Assert.Contains("B002", content);
        }

        [Fact]
        public async Task WriteStreamDictionaryCsvAsync_EmptyRows_ReturnsEmptyStream()
        {
            var ms = await _helper.WriteStreamDictionaryCsvAsync(
                new List<Dictionary<string, object?>>());

            // DictionaryCsvWriter returns early when rows.Count == 0
            Assert.Equal(0, ms.Length);
        }

        // -------------------------------------------------------
        // WriteDictionaryCsvAsync (file path)
        // -------------------------------------------------------
        [Fact]
        public async Task WriteDictionaryCsvAsync_ValidPath_WritesFile()
        {
            var path = Path.ChangeExtension(Path.GetTempFileName(), ".csv");

            try
            {
                await _helper.WriteDictionaryCsvAsync(SomeDictionaryRows(), path);

                Assert.True(File.Exists(path));
                var content = await File.ReadAllTextAsync(path);
                Assert.Contains("SKU", content);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task WriteDictionaryCsvAsync_InvalidPath_Throws()
        {
            var invalidPath = "Z:\\NonExistentDrive\\file.csv";

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _helper.WriteDictionaryCsvAsync(SomeDictionaryRows(), invalidPath));
        }

        // -------------------------------------------------------
        // WriteStreamDictionaryExcel
        // -------------------------------------------------------
        [Fact]
        public void WriteStreamDictionaryExcel_WithRows_ReturnsNonEmptyStream()
        {
            var ms = _helper.WriteStreamDictionaryExcel(SomeDictionaryRows());

            Assert.NotNull(ms);
            Assert.True(ms.Length > 0);
            Assert.Equal(0, ms.Position);
        }

        [Fact]
        public void WriteStreamDictionaryExcel_WithRows_ContainsHeadersAndData()
        {
            var ms = _helper.WriteStreamDictionaryExcel(SomeDictionaryRows());

            ms.Position = 0;
            using var workbook = new XLWorkbook(ms);
            var sheet = workbook.Worksheet("Export");

            Assert.Equal("SKU", sheet.Cell(1, 1).Value.ToString());
            Assert.Equal("Qty", sheet.Cell(1, 2).Value.ToString());
            Assert.Equal("A001", sheet.Cell(2, 1).Value.ToString());
            Assert.Equal("B002", sheet.Cell(3, 1).Value.ToString());
        }

        [Fact]
        public void WriteStreamDictionaryExcel_EmptyRows_ReturnsWorkbookWithEmptySheet()
        {
            var ms = _helper.WriteStreamDictionaryExcel(new List<Dictionary<string, object?>>());

            ms.Position = 0;
            using var workbook = new XLWorkbook(ms);
            var sheet = workbook.Worksheet("Export");

            Assert.NotNull(sheet);
            Assert.Equal("", sheet.Cell(1, 1).Value.ToString());
            Assert.Equal("", sheet.Cell(2, 1).Value.ToString());
        }

        // -------------------------------------------------------
        // WriteDictionaryExcel (file path)
        // -------------------------------------------------------
        [Fact]
        public void WriteDictionaryExcel_ValidPath_WritesFile()
        {
            var path = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");

            try
            {
                _helper.WriteDictionaryExcel(SomeDictionaryRows(), path);

                Assert.True(File.Exists(path));
                Assert.True(new FileInfo(path).Length > 0);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WriteDictionaryExcel_InvalidPath_Throws()
        {
            var invalidPath = "Z:\\NonExistentDrive\\file.xlsx";

            Assert.ThrowsAny<Exception>(() =>
                _helper.WriteDictionaryExcel(SomeDictionaryRows(), invalidPath));
        }
    }
}