using ClosedXML.Excel;
using CsvHelper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Export;
using Infrastructure.Context;
using Infrastructure.Services.Export.Records;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Globalization;

namespace Infrastructure.Services.Export
{
    public class ExportService(DataContext context) : IExportService
    {
        private const int Timeout = 120;
        private readonly DataContext _context = context;
        public async Task ExportAsync(ExportProfile profile, Guid companyId, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(profile.CustomSource))
            {
                await ExportCustomAsync(profile, companyId, ct);
                return;
            }

            switch (profile.ExportType)
            {
                case ExportType.StockLevels:
                    var stockRecords = await GetStockLevelRecordsAsync(companyId, ct);
                    await WriteFileAsync(stockRecords, profile, ct);
                    break;

                case ExportType.SalesAggregates:
                    var salesRecords = await GetSalesAggregateRecordsAsync(companyId, ct);
                    await WriteFileAsync(salesRecords, profile, ct);
                    break;

                case ExportType.Forecasts:
                    var forecastRecords = await GetForecastRecordsAsync(companyId, ct);
                    await WriteFileAsync(forecastRecords, profile, ct);
                    break;

                case ExportType.PurchaseForecasts:
                    var purchaseForecastRecords = await GetPurchaseForecastRecordsAsync(companyId, ct);
                    await WriteFileAsync(purchaseForecastRecords, profile, ct);
                    break;

                case ExportType.ConfirmedOrderAggregates:
                    var confirmedOrderRecords = await GetConfirmedOrderAggregateRecordsAsync(companyId, ct);
                    await WriteFileAsync(confirmedOrderRecords, profile, ct);
                    break;

                case ExportType.ProductKpis:
                    var kpiRecords = await GetProductKpiRecordsAsync(companyId, ct);
                    await WriteFileAsync(kpiRecords, profile, ct);
                    break;

                case ExportType.Seasonality:
                    var seasonalityRecords = await GetSeasonalityRecordsAsync(companyId, ct);
                    await WriteFileAsync(seasonalityRecords, profile, ct);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported export type: {profile.ExportType}");
            }
        }

        private async Task<List<T>> QueryAsync<T>(
            string sql,
            Func<DbDataReader, T> map,
            CancellationToken ct,
            params SqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = Timeout;

            foreach (var p in parameters)
                command.Parameters.Add(p);

            await using var reader = await command.ExecuteReaderAsync(ct);
            var results = new List<T>();

            while (await reader.ReadAsync(ct))
                results.Add(map(reader));

            return results;
        }

        private async Task<List<ConfirmedOrderAggregateRecord>> GetConfirmedOrderAggregateRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
                    SKU,
                    PeriodType,
                    PeriodStartDate,
                    PeriodEndDate,
                    TotalConfirmedQuantity,
                    TotalConfirmedRevenue
                FROM ConfirmedOrderAggregates
                INNER JOIN Products ON Id = ProductId
                WHERE CompanyId = @CompanyId
                ORDER BY SKU, PeriodType, PeriodStartDate
                ",
                r => new ConfirmedOrderAggregateRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    PeriodType = r.GetString(r.GetOrdinal("PeriodType")),
                    PeriodStartDate = r.GetDateTime(r.GetOrdinal("PeriodStartDate")),
                    PeriodEndDate = r.GetDateTime(r.GetOrdinal("PeriodEndDate")),
                    TotalConfirmedQuantity = r.GetInt32(r.GetOrdinal("TotalConfirmedQuantity")),
                    TotalConfirmedRevenue = r.GetDecimal(r.GetOrdinal("TotalConfirmedRevenue"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<ForecastRecord>> GetForecastRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
                    SKU,
                    PeriodType,
                    PeriodStartDate,
                    PeriodEndDate,
                    PredictedQuantity,
                    PredictedRevenue
                FROM Forecasts
                INNER JOIN Products ON Id = ProductId
                WHERE CompanyId = @CompanyId
                ORDER BY SKU, PeriodType, PeriodStartDate
                ",
                r => new ForecastRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    PeriodType = r.GetString(r.GetOrdinal("PeriodType")),
                    PeriodStartDate = r.GetDateTime(r.GetOrdinal("PeriodStartDate")),
                    PeriodEndDate = r.GetDateTime(r.GetOrdinal("PeriodEndDate")),
                    PredictedQuantity = r.GetDecimal(r.GetOrdinal("PredictedQuantity")),
                    PredictedRevenue = r.GetDecimal(r.GetOrdinal("PredictedRevenue"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<ProductKpiRecord>> GetProductKpiRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
	                CASE 
                        WHEN SchedulerStatus = 0 THEN 'Pending'
                        WHEN SchedulerStatus = 1 THEN 'Running'
                        WHEN SchedulerStatus = 2 THEN 'Succeeded'
                        WHEN SchedulerStatus = 3 THEN 'Failed'
                    END AS SchedulerStatus,
	                TotalProductCount,
	                BelowSafetyStockCount,
	                RecommendedOrdersCount,
	                WithoutStockAtLeadTimeCount,
	                WithoutForecastCount
                FROM ProductKpis
                WHERE CompanyId = @CompanyId
                ",
                r => new ProductKpiRecord
                {
                    SchedulerStatus = r.GetString(r.GetOrdinal("SchedulerStatus")),
                    TotalProductCount = r.GetInt32(r.GetOrdinal("TotalProductCount")),
                    BelowSafetyStockCount = r.GetInt32(r.GetOrdinal("BelowSafetyStockCount")),
                    RecommendedOrdersCount = r.GetInt32(r.GetOrdinal("RecommendedOrdersCount")),
                    WithoutStockAtLeadTimeCount = r.GetInt32(r.GetOrdinal("WithoutStockAtLeadTimeCount")),
                    WithoutForecastCount = r.GetInt32(r.GetOrdinal("WithoutForecastCount"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<PurchaseForecastRecord>> GetPurchaseForecastRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
	                SKU,
	                WeekStartDate,
	                WeekEndDate,
	                WeekIndex,
	                ForecastQuantity,
	                QuantityCommitted,
					QuantityOnOrder,
					ProjectedStock,
					NeedsReorder,
					RecommendedOrderQuantity,
					RecommendedOrderDate
                FROM PurchaseForecasts
                INNER JOIN Products ON Id = ProductId
                WHERE CompanyId = @CompanyId
                ORDER BY SKU, WeekIndex
                ",
                r => new PurchaseForecastRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    WeekStartDate = r.GetDateTime(r.GetOrdinal("WeekStartDate")),
                    WeekEndDate = r.GetDateTime(r.GetOrdinal("WeekEndDate")),
                    WeekIndex = r.GetInt32(r.GetOrdinal("WeekIndex")),
                    ForecastQuantity = r.GetDecimal(r.GetOrdinal("ForecastQuantity")),
                    QuantityCommitted = r.GetInt32(r.GetOrdinal("QuantityCommitted")),
                    QuantityOnOrder = r.GetInt32(r.GetOrdinal("QuantityOnOrder")),
                    ProjectedStock = r.GetInt32(r.GetOrdinal("ProjectedStock")),
                    NeedsReorder = r.GetBoolean(r.GetOrdinal("NeedsReorder")),
                    RecommendedOrderQuantity = r.GetInt32(r.GetOrdinal("RecommendedOrderQuantity")),
                    RecommendedOrderDate = r.GetDateTime(r.GetOrdinal("RecommendedOrderDate"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<SalesAggregateRecord>> GetSalesAggregateRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
	                SKU,
	                PeriodType,
	                PeriodStartDate,
	                PeriodEndDate,
	                TotalQuantity,
	                TotalRevenue
                FROM SalesAggregates
                INNER JOIN Products ON Id = ProductId
                WHERE CompanyId = @CompanyId
                ORDER BY SKU, PeriodType, PeriodStartDate
                ",
                r => new SalesAggregateRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    PeriodType = r.GetString(r.GetOrdinal("PeriodType")),
                    PeriodStartDate = r.GetDateTime(r.GetOrdinal("PeriodStartDate")),
                    PeriodEndDate = r.GetDateTime(r.GetOrdinal("PeriodEndDate")),
                    TotalQuantity = r.GetInt32(r.GetOrdinal("TotalQuantity")),
                    TotalRevenue = r.GetDecimal(r.GetOrdinal("TotalRevenue"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<SeasonalityRecord>> GetSeasonalityRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT
	                SKU,
	                s.PeriodStartDate,
					f.PredictedQuantity,
					s.Factor
                FROM Seasonality s
                INNER JOIN Products p 
					ON p.Id = s.ProductId
				INNER JOIN Forecasts f 
					ON p.Id = f.ProductId 
					AND f.PeriodStartDate = s.PeriodStartDate
					AND f.PeriodType = 'Monthly'
				WHERE CompanyId = @CompanyId 
                ORDER BY SKU, PeriodStartDate
                ",
                r => new SeasonalityRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    PeriodStartDate = r.GetDateTime(r.GetOrdinal("PeriodStartDate")),
                    PredictedQuantity = r.GetDecimal(r.GetOrdinal("PredictedQuantity")),
                    Factor = r.GetDecimal(r.GetOrdinal("Factor"))
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        private async Task<List<StockLevelRecord>> GetStockLevelRecordsAsync(
            Guid companyId, CancellationToken ct)
        {
            return await QueryAsync(
                @"
                SELECT 
	                SKU,
	                QuantityOnHand,
	                QuantityOnOrder,
	                QuantityCommitted,
	                NetStockPosition,
	                ForecastQuantity,
	                ReorderPoint,
	                NeedsReorder,
	                RecommendedOrderQuantity
                FROM StockLevels
                INNER JOIN Products ON Id = ProductId
                WHERE CompanyId = @CompanyId
                ORDER BY SKU
                ",
                r => new StockLevelRecord
                {
                    SKU = r.GetString(r.GetOrdinal("SKU")),
                    QuantityOnHand = r.GetInt32(r.GetOrdinal("QuantityOnHand")),
                    QuantityOnOrder = r.GetInt32(r.GetOrdinal("QuantityOnOrder")),
                    QuantityCommitted = r.GetInt32(r.GetOrdinal("QuantityCommitted")),
                    NetStockPosition = r.GetDecimal(r.GetOrdinal("NetStockPosition")),
                    ForecastQuantity = r.GetDecimal(r.GetOrdinal("ForecastQuantity")),
                    ReorderPoint = r.GetInt32(r.GetOrdinal("ReorderPoint")),
                    NeedsReorder = r.GetBoolean(r.GetOrdinal("NeedsReorder")),
                    RecommendedOrderQuantity = r.GetInt32(r.GetOrdinal("RecommendedOrderQuantity")),
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        //private async Task<List<SummaryRecord>> GetSummaryRecordsAsync(
        //    Guid companyId, CancellationToken ct)
        //{
        //    return new List<SummaryRecord> { };
        //}

        private static string TryGetValidTargetPath(ExportProfile profile)
        {
            string? targetPath = profile.TargetPath;

            if (targetPath == null)
                throw new InvalidOperationException("TargetPath must be filled in");

            if (!targetPath.EndsWith(".csv") && !targetPath.EndsWith(".xlsx"))
                throw new InvalidOperationException("Extension of targetPath is not supported");

            if (targetPath.EndsWith(".csv") && profile.ExportFileType == FileType.Excel)
                throw new InvalidOperationException("Extension of the targetPath is .csv but export file type is excel");

            if (targetPath.EndsWith(".xlsx") && profile.ExportFileType == FileType.Csv)
                throw new InvalidOperationException("Extension of the targetPath is .xlsx but export file type is csv");

            return targetPath;
        }

        // --- File writing ---
        private async Task WriteFileAsync<T>(
            List<T> records, ExportProfile profile, CancellationToken ct)
        {
            string targetPath = TryGetValidTargetPath(profile);

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (profile.ExportFileType == FileType.Csv)
                await WriteCsvAsync(records, targetPath, ct);
            else
                WriteExcel(records, targetPath);
        }

        private static async Task WriteCsvAsync<T>(
            List<T> records, string targetPath, CancellationToken ct)
        {
            await using var writer = new StreamWriter(targetPath);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(records, ct);
        }

        private static void WriteExcel<T>(List<T> records, string targetPath)
        {
            using var workbook = new XLWorkbook();
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

            workbook.SaveAs(targetPath);
        }

        // --- Custom export ---
        private async Task ExportCustomAsync(
            ExportProfile profile, Guid companyId, CancellationToken ct)
        {
            string targetPath = TryGetValidTargetPath(profile);

            if (!profile.CustomSource!.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                && !profile.CustomSource.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Invalid custom source name '{profile.CustomSource}'. Must start with 'custom_' or 'dbo.custom_'.");

            // Validate existence and type match
            var objectType = await _context.Database
                .SqlQueryRaw<string>(
                    @"SELECT type AS Value FROM sys.objects WHERE name = @customSource",
                    new SqlParameter("@customSource", profile.CustomSource.Replace("dbo.",""))
                )
                .SingleOrDefaultAsync(ct);

            if (objectType == null)
                throw new InvalidOperationException(
                    $"Custom source '{profile.CustomSource}' does not exist in the database.");

            objectType = objectType.Trim();

            if (objectType == "V" && profile.CustomType != CustomType.View)
                throw new InvalidOperationException(
                    $"'{profile.CustomSource}' is a view but CustomType is set to Procedure.");

            if (objectType == "P" && profile.CustomType != CustomType.Procedure)
                throw new InvalidOperationException(
                    $"'{profile.CustomSource}' is a stored procedure but CustomType is set to View.");

            if (objectType != "V" && objectType != "P")
                throw new InvalidOperationException(
                    $"'{profile.CustomSource}' exists but is not a view or stored procedure (type: {objectType}).");

            var commandText = profile.CustomType == CustomType.View
                ? $"SELECT * FROM {profile.CustomSource} WHERE CompanyId = @companyId"
                : $"EXEC {profile.CustomSource} @companyId";

            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(ct);

            List<Dictionary<string, object?>> rows;

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = Timeout;

                var param = command.CreateParameter();
                param.ParameterName = "@companyId";
                param.Value = companyId;
                command.Parameters.Add(param);

                await using var reader = await command.ExecuteReaderAsync(ct);
                rows = new List<Dictionary<string, object?>>();

                var columnNames = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => reader.GetName(i))
                    .Where(name => !name.Equals("CompanyId", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                while (await reader.ReadAsync(ct))
                {
                    var row = new Dictionary<string, object?>();
                    foreach (var col in columnNames)
                        row[col] = reader.IsDBNull(reader.GetOrdinal(col)) ? null : reader.GetValue(reader.GetOrdinal(col));
                    rows.Add(row);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to execute custom source '{profile.CustomSource}': {ex.Message}", ex);
            }

            if (profile.ExportFileType == FileType.Csv)
                await WriteDictionaryCsvAsync(rows, targetPath, ct);
            else
                WriteDictionaryExcel(rows, targetPath);
        }

        private static async Task WriteDictionaryCsvAsync(
            List<Dictionary<string, object?>> rows, string targetPath, CancellationToken ct)
        {
            await using var writer = new StreamWriter(targetPath);
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

        private static void WriteDictionaryExcel(
            List<Dictionary<string, object?>> rows, string targetPath)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Export");

            if (rows.Count == 0)
            {
                workbook.SaveAs(targetPath);
                return;
            }

            var headers = rows[0].Keys.ToList();
            for (int i = 0; i < headers.Count; i++)
                sheet.Cell(1, i + 1).Value = headers[i];

            for (int row = 0; row < rows.Count; row++)
                for (int col = 0; col < headers.Count; col++)
                    sheet.Cell(row + 2, col + 1).Value =
                        XLCellValue.FromObject(rows[row][headers[col]]);

            workbook.SaveAs(targetPath);
        }
    }
}
