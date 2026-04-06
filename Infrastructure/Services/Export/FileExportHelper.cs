using ClosedXML.Excel;
using CsvHelper;
using Domain.Enums;
using Domain.Interfaces.Services.Export;
using System.Globalization;

namespace Infrastructure.Services.Export
{
    public class FileExportHelper : IFileExportHelper
    {
        public FileType GetFileTypeFromPath(string targetPath)
        {
            string? extension = targetPath != null ? Path.GetExtension(targetPath).ToLowerInvariant() : null;

            return extension switch
            {
                ".csv" => FileType.Csv,
                ".xlsx" => FileType.Excel,
                _ => throw new InvalidOperationException(
                    $"Unsupported file extension '{extension}'. Supported extensions are .csv and .xlsx.")
            };
        }

        public async Task WriteCsvAsync<T>(
            List<T> records, 
            string targetPath, 
            CancellationToken ct
        )
        {
            await using var writer = new StreamWriter(targetPath);
            await CsvWriter(writer, records, ct);
        }

        public async Task<MemoryStream> WriteStreamCsvAsync<T>(
            List<T> records,
            CancellationToken ct
        )
        {
            var ms = new MemoryStream();
            await using var writer = new StreamWriter(ms, leaveOpen: true);
            await CsvWriter(writer, records, ct);
            ms.Position = 0;
            return ms;
        }

        public void WriteExcel<T>(List<T> records, string targetPath)
        {
            using var workbook = ExcelWriter(records);

            workbook.SaveAs(targetPath);
        }

        public MemoryStream WriteStreamExcel<T>(List<T> records)
        {
            var ms = new MemoryStream();
            using var workbook = ExcelWriter(records);
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }

        public async Task WriteDictionaryCsvAsync(List<Dictionary<string, object?>> rows, string targetPath)
        {
            await using var writer = new StreamWriter(targetPath);
            await DictionaryCsvWriter(writer, rows);
        }

        public async Task<MemoryStream> WriteStreamDictionaryCsvAsync(
            List<Dictionary<string,
            object?>> rows
        )
        {
            var ms = new MemoryStream();
            await using var writer = new StreamWriter(ms, leaveOpen: true);
            await DictionaryCsvWriter(writer, rows);
            ms.Position = 0;
            return ms;
        }

        public void WriteDictionaryExcel(List<Dictionary<string, object?>> rows, string targetPath)
        {
            using var workbook = DictionaryExcelWriter(rows);

            workbook.SaveAs(targetPath);
        }

        public MemoryStream WriteStreamDictionaryExcel(List<Dictionary<string, object?>> rows)
        {
            var ms = new MemoryStream();
            using var workbook = DictionaryExcelWriter(rows);
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }

        private static async Task CsvWriter<T>(
            StreamWriter? writer,
            List<T> records,
            CancellationToken ct
        )
        {
            if (writer != null)
            {
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                await csv.WriteRecordsAsync(records, ct);
            }
        }

        private static XLWorkbook ExcelWriter<T>(List<T> records)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Export");

            var properties = typeof(T).GetProperties();

            // Write header row
            for (int i = 0; i < properties.Length; i++)
                sheet.Cell(1, i + 1).Value = properties[i].Name;

            // Write data rows
            for (int row = 0; row < records.Count; row++)
                for (int col = 0; col < properties.Length; col++)
                    sheet.Cell(row + 2, col + 1).Value =
                        XLCellValue.FromObject(properties[col].GetValue(records[row]));

            return workbook;
        }

        private static async Task DictionaryCsvWriter(StreamWriter? writer, List<Dictionary<string, object?>> rows)
        {
            if (writer != null)
            {
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                if (rows.Count == 0) return;

                foreach (var key in rows[0].Keys)
                    csv.WriteField(key);
                await csv.NextRecordAsync();

                foreach (var row in rows)
                {
                    foreach (var value in row.Values)
                        csv.WriteField(value);
                    await csv.NextRecordAsync();
                }
            }
        }

        private static XLWorkbook DictionaryExcelWriter(List<Dictionary<string, object?>> rows)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Export");

            var headers = rows[0].Keys.ToList();
            for (int i = 0; i < headers.Count; i++)
                sheet.Cell(1, i + 1).Value = headers[i];

            for (int row = 0; row < rows.Count; row++)
                for (int col = 0; col < headers.Count; col++)
                    sheet.Cell(row + 2, col + 1).Value =
                        XLCellValue.FromObject(rows[row][headers[col]]);

            return workbook;
        }
    }
}
