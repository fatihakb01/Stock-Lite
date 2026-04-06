using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Export;
using Domain.Interfaces.Services.Export.Records;
using Infrastructure.Context;
using Infrastructure.Services.Export.Records;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Infrastructure.Services.Export
{
    public class ExportService(
        DataContext context,
        IFileExportHelper helper
    ) : IExportService
    {
        private const int Timeout = 120;
        private readonly DataContext _context = context;
        private readonly IFileExportHelper _helper = helper;

        public async Task<ExportResult> ExportToStreamAsync(
            ExportProfile profile, 
            FileType fileType, 
            Guid companyId, 
            CancellationToken ct
        )
        {
            MemoryStream ms;

            if (!string.IsNullOrWhiteSpace(profile.CustomSource))
            {
                ms = await ExportCustomToStreamAsync(profile, fileType, companyId, ct);
            }
            else
            {
                ms = profile.ExportType switch
                {
                    ExportType.StockLevels => await WriteToStreamAsync(
                        await GetStockLevelRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.SalesAggregates => await WriteToStreamAsync(
                        await GetSalesAggregateRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.Forecasts => await WriteToStreamAsync(
                        await GetForecastRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.PurchaseForecasts => await WriteToStreamAsync(
                        await GetPurchaseForecastRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.ConfirmedOrderAggregates => await WriteToStreamAsync(
                        await GetConfirmedOrderAggregateRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.ProductKpis => await WriteToStreamAsync(
                        await GetProductKpiRecordsAsync(companyId, ct), fileType, ct),

                    ExportType.Seasonality => await WriteToStreamAsync(
                        await GetSeasonalityRecordsAsync(companyId, ct), fileType, ct),

                    _ => throw new InvalidOperationException(
                        $"Unsupported export type: {profile.ExportType}")
                };
            }

            var extension = fileType == FileType.Csv ? "csv" : "xlsx";
            var contentType = fileType == FileType.Csv
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return new ExportResult
            {
                Content = ms.ToArray(),
                FileName = $"{profile.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}",
                ContentType = contentType
            };
        }

        public async Task ExportToFileAsync(ExportProfile profile, Guid companyId, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(profile.CustomSource))
            {
                await ExportCustomAsync(profile, companyId, ct);
                return;
            }

            var task = profile.ExportType switch
            {
                ExportType.StockLevels => WriteFileAsync(
                    await GetStockLevelRecordsAsync(companyId, ct), profile, ct),

                ExportType.SalesAggregates => WriteFileAsync(
                    await GetSalesAggregateRecordsAsync(companyId, ct), profile, ct),

                ExportType.Forecasts => WriteFileAsync(
                    await GetForecastRecordsAsync(companyId, ct), profile, ct),

                ExportType.PurchaseForecasts => WriteFileAsync(
                    await GetPurchaseForecastRecordsAsync(companyId, ct), profile, ct),

                ExportType.ConfirmedOrderAggregates => WriteFileAsync(
                    await GetConfirmedOrderAggregateRecordsAsync(companyId, ct), profile, ct),

                ExportType.ProductKpis => WriteFileAsync(
                    await GetProductKpiRecordsAsync(companyId, ct), profile, ct),

                ExportType.Seasonality => WriteFileAsync(
                    await GetSeasonalityRecordsAsync(companyId, ct), profile, ct),

                _ => throw new InvalidOperationException($"Unsupported export type: {profile.ExportType}")
            };

            await task;
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
                    SKU = r.GetRefField<string>("SKU"),
                    PeriodType = r.GetRefField<string>("PeriodType"),
                    PeriodStartDate = r.GetField<DateTime>("PeriodStartDate"),
                    PeriodEndDate = r.GetField<DateTime>("PeriodEndDate"),
                    TotalConfirmedQuantity = r.GetField<int>("TotalConfirmedQuantity"),
                    TotalConfirmedRevenue = r.GetField<decimal>("TotalConfirmedRevenue")
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
                    SKU = r.GetRefField<string>("SKU"),
                    PeriodType = r.GetRefField<string>("PeriodType"),
                    PeriodStartDate = r.GetField<DateTime>("PeriodStartDate"),
                    PeriodEndDate = r.GetField<DateTime>("PeriodEndDate"),
                    PredictedQuantity = r.GetField<decimal>("PredictedQuantity"),
                    PredictedRevenue = r.GetField<decimal>("PredictedRevenue")
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
                    SchedulerStatus = r.GetRefField<string>("SchedulerStatus"),
                    TotalProductCount = r.GetField<int>("TotalProductCount"),
                    BelowSafetyStockCount = r.GetField<int>("BelowSafetyStockCount"),
                    RecommendedOrdersCount = r.GetField<int>("RecommendedOrdersCount"),
                    WithoutStockAtLeadTimeCount = r.GetField<int>("WithoutStockAtLeadTimeCount"),
                    WithoutForecastCount = r.GetField<int>("WithoutForecastCount")
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
                    SKU = r.GetRefField<string>("SKU"),
                    WeekStartDate = r.GetField<DateTime>("WeekStartDate"),
                    WeekEndDate = r.GetField<DateTime>("WeekEndDate"),
                    WeekIndex = r.GetField<int>("WeekIndex"),
                    ForecastQuantity = r.GetField<decimal>("ForecastQuantity"),
                    QuantityCommitted = r.GetField<int>("QuantityCommitted"),
                    QuantityOnOrder = r.GetField<int>("QuantityOnOrder"),
                    ProjectedStock = r.GetField<decimal>("ProjectedStock"),
                    NeedsReorder = r.GetField<bool>("NeedsReorder"),
                    RecommendedOrderQuantity = r.GetField<int>("RecommendedOrderQuantity"),
                    RecommendedOrderDate = r.GetNullableField<DateTime>("RecommendedOrderDate")
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
                    SKU = r.GetRefField<string>("SKU"),
                    PeriodType = r.GetRefField<string>("PeriodType"),
                    PeriodStartDate = r.GetField<DateTime>("PeriodStartDate"),
                    PeriodEndDate = r.GetField<DateTime>("PeriodEndDate"),
                    TotalQuantity = r.GetField<int>("TotalQuantity"),
                    TotalRevenue = r.GetField<decimal>("TotalRevenue")
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
                    SKU = r.GetRefField<string>("SKU"),
                    PeriodStartDate = r.GetField<DateTime>("PeriodStartDate"),
                    PredictedQuantity = r.GetField<decimal>("PredictedQuantity"),
                    Factor = r.GetField<decimal>("Factor")
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
                    SKU = r.GetRefField<string>("SKU"),
                    QuantityOnHand = r.GetField<int>("QuantityOnHand"),
                    QuantityOnOrder = r.GetField<int>("QuantityOnOrder"),
                    QuantityCommitted = r.GetField<int>("QuantityCommitted"),
                    NetStockPosition = r.GetField<decimal>("NetStockPosition"),
                    ForecastQuantity = r.GetField<decimal>("ForecastQuantity"),
                    ReorderPoint = r.GetField<int>("ReorderPoint"),
                    NeedsReorder = r.GetField<bool>("NeedsReorder"),
                    RecommendedOrderQuantity = r.GetField<int>("RecommendedOrderQuantity")
                },
                ct,
                new SqlParameter("@CompanyId", companyId)
            );
        }

        // --- File writing ---
        private async Task WriteFileAsync<T>(
            List<T> records, ExportProfile profile, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(profile.TargetPath))
                throw new InvalidOperationException("TargetPath must be filled in.");

            var fileType = _helper.GetFileTypeFromPath(profile.TargetPath);

            var directory = Path.GetDirectoryName(profile.TargetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (fileType == FileType.Csv)
                await _helper.WriteCsvAsync(records, profile.TargetPath, ct);
            else
                _helper.WriteExcel(records, profile.TargetPath);
        }

        // --- Stream writing ---
        private async Task<MemoryStream> WriteToStreamAsync<T>(
            List<T> records, FileType fileType, CancellationToken ct)
        {
            return fileType == FileType.Csv
                ? await _helper.WriteStreamCsvAsync(records, ct)
                : _helper.WriteStreamExcel(records);
        }

        // --- Custom export ---
        private async Task<MemoryStream> ExportCustomToStreamAsync(
            ExportProfile profile, FileType fileType, Guid companyId, CancellationToken ct)
        {
            var rows = await FetchCustomRowsAsync(profile, companyId, ct);

            return fileType == FileType.Csv
                ? await _helper.WriteStreamDictionaryCsvAsync(rows)
                : _helper.WriteStreamDictionaryExcel(rows);
        }

        private async Task ExportCustomAsync(
            ExportProfile profile, Guid companyId, CancellationToken ct)
        {
            var fileType = _helper.GetFileTypeFromPath(profile.TargetPath!);
            var rows = await FetchCustomRowsAsync(profile, companyId, ct);

            var directory = Path.GetDirectoryName(profile.TargetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            if (fileType == FileType.Csv)
                await _helper.WriteDictionaryCsvAsync(rows, profile.TargetPath!);
            else
                _helper.WriteDictionaryExcel(rows, profile.TargetPath!);
        }

        private async Task<List<Dictionary<string, object?>>> FetchCustomRowsAsync(
            ExportProfile profile, Guid companyId, CancellationToken ct)
        {
            if (!profile.CustomSource!.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                && !profile.CustomSource.StartsWith("dbo.custom_", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Invalid custom source name '{profile.CustomSource}'. Must start with 'custom_' or 'dbo.custom_'.");

            // Validate existence and type match
            var objectType = await _context.Database
                .SqlQueryRaw<string>(
                    @"SELECT type AS Value FROM sys.objects WHERE name = @customSource",
                    new SqlParameter("@customSource", profile.CustomSource.Replace("dbo.", ""))
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
                    {
                        var ordinal = reader.GetOrdinal(col);
                        row[col] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                    }
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

            return rows;
        }
    }
}
