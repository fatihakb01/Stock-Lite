using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer
{
    public static class ForecastsInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            // Delete forecast in the past
            await DeletePastForecastsProcAsync(context, logger);

            // Insert new forecast records if no forecast exists
            await InsertDailyForecastsProcAsync(context, logger);
            await InsertWeeklyForecastsProcAsync(context, logger);
            await InsertMonthlyForecastsProcAsync(context, logger);
            await InsertForecastsProcAsync(context, logger);

            // Update forecast records if they already exist
            await UpdateDailyForecastsProcAsync(context, logger);
            await UpdateWeeklyForecastsProcAsync(context, logger);
            await UpdateMonthlyForecastsProcAsync(context, logger);
            await UpdateForecastsProcAsync(context, logger);
        }

        private static async Task DeletePastForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.DeletePastForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.DeletePastForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY,
                @PeriodType NVARCHAR(50)
            AS
            BEGIN
                SET NOCOUNT ON;

                DELETE f
                FROM Forecasts f
                JOIN Products p ON p.Id = f.ProductId
                WHERE p.CompanyId = @CompanyId
                AND f.PeriodEndDate < CONVERT(DATE, SYSUTCDATETIME())
                AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                AND f.PeriodType = @PeriodType;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertDailyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertDailyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertDailyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.DeletePastForecasts @CompanyId, @ProductIds, 'Daily';

                WITH AvgDaily AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Daily'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(DAY, -30, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                    GROUP BY sa.ProductId
                ),
                ForecastDays AS (
                    SELECT 
					    'Daily' AS PeriodType,
						DATEADD(DAY, v.number - 1, CONVERT(DATE, SYSUTCDATETIME())) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number BETWEEN 1 AND 30
                )
                INSERT INTO Forecasts (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, PredictedQuantity, PredictedRevenue, Confidence)
                SELECT 
                    p.Id,
                    'Daily',
                    fd.PeriodStartDate,
                    fd.PeriodStartDate,
                    ISNULL(a.AvgQty, 0),
                    ISNULL(a.AvgRev, 0),
                    0.50
                FROM Products p
                CROSS JOIN ForecastDays fd
                LEFT JOIN AvgDaily a ON a.ProductId = p.Id
                LEFT JOIN Forecasts f
                    ON f.ProductId = p.Id
                    AND f.PeriodType = fd.PeriodType
                    AND f.PeriodStartDate = fd.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND f.ProductId IS NULL;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertWeeklyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertWeeklyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertWeeklyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.DeletePastForecasts @CompanyId, @ProductIds, 'Weekly';

                WITH AvgWeekly AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Weekly'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(WEEK, -13, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                    GROUP BY sa.ProductId
                ),
                ForecastWeeks AS (
                    SELECT 'Weekly' AS PeriodType, 
                    DATEADD(WEEK, v.number, DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETUTCDATE() AS DATE)) + 1, CAST(GETUTCDATE() AS DATE))) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number < 13
                )
                INSERT INTO Forecasts (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, PredictedQuantity, PredictedRevenue, Confidence)
                SELECT 
                    p.Id,
                    'Weekly',
                    fw.PeriodStartDate,
                    DATEADD(DAY, 6, fw.PeriodStartDate),
                    ISNULL(a.AvgQty, 0),
                    ISNULL(a.AvgRev, 0),
                    0.50
                FROM Products p
                CROSS JOIN ForecastWeeks fw
                LEFT JOIN AvgWeekly a ON a.ProductId = p.Id
                LEFT JOIN Forecasts f
                    ON f.ProductId = p.Id
                    AND f.PeriodType = fw.PeriodType
                    AND f.PeriodStartDate = fw.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND f.ProductId IS NULL;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertMonthlyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertMonthlyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertMonthlyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.DeletePastForecasts @CompanyId, @ProductIds, 'Monthly';

                WITH AvgMonthly AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Monthly'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(MONTH, -12, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                    GROUP BY sa.ProductId
                ),
                ForecastMonths AS (
                    SELECT 
                        'Monthly' AS PeriodType,
						DATEFROMPARTS(YEAR(DATEADD(MONTH, v.number, DATEADD(MONTH, -1, GETUTCDATE()))), MONTH(DATEADD(MONTH, v.number, DATEADD(MONTH, -1, GETUTCDATE()))), 1) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number BETWEEN 1 AND 12
                )
                INSERT INTO Forecasts (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, PredictedQuantity, PredictedRevenue, Confidence)
                SELECT 
                    p.Id,
                    'Monthly',
                    fm.PeriodStartDate,
                    EOMONTH(fm.PeriodStartDate),
                    ISNULL(a.AvgQty, 0),
                    ISNULL(a.AvgRev, 0),
                    0.50
                FROM Products p
                CROSS JOIN ForecastMonths fm
                LEFT JOIN AvgMonthly a ON a.ProductId = p.Id
                LEFT JOIN Forecasts f
                    ON f.ProductId = p.Id
                    AND f.PeriodType = fm.PeriodType
                    AND f.PeriodStartDate = fm.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND f.ProductId IS NULL;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.InsertDailyForecasts @CompanyId, @ProductIds;
                EXEC dbo.InsertWeeklyForecasts @CompanyId, @ProductIds;
                EXEC dbo.InsertMonthlyForecasts @CompanyId, @ProductIds;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateDailyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateDailyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateDailyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                WITH AvgDaily AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Daily'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(DAY, -30, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                    GROUP BY sa.ProductId
                )
                UPDATE f
                SET f.PredictedQuantity = ISNULL(a.AvgQty, 0),
                    f.PredictedRevenue = ISNULL(a.AvgRev, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                JOIN Products p ON p.Id = f.ProductId
                LEFT JOIN AvgDaily a ON a.ProductId = f.ProductId
                WHERE p.CompanyId = @CompanyId
                AND f.PeriodType = 'Daily'
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateWeeklyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateWeeklyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateWeeklyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                WITH AvgWeekly AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Weekly'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(WEEK, -13, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds pid WHERE pid.Id = p.Id)
                    GROUP BY sa.ProductId
                )
                UPDATE f
                SET 
                    f.PredictedQuantity = ISNULL(a.AvgQty, 0),
                    f.PredictedRevenue = ISNULL(a.AvgRev, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                JOIN Products p ON p.Id = f.ProductId
                LEFT JOIN AvgWeekly a ON a.ProductId = f.ProductId
                WHERE p.CompanyId = @CompanyId
                AND f.PeriodType = 'Weekly'
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateMonthlyForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateMonthlyForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateMonthlyForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                WITH AvgMonthly AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Monthly'
                    AND p.CompanyId = @CompanyId
                    AND sa.PeriodStartDate >= DATEADD(MONTH, -12, CONVERT(DATE, SYSUTCDATETIME()))
                    AND EXISTS (SELECT 1 FROM @ProductIds WHERE Id = p.Id)
                    GROUP BY sa.ProductId
                )
                UPDATE f
                SET 
                    f.PredictedQuantity = ISNULL(a.AvgQty, 0),
                    f.PredictedRevenue = ISNULL(a.AvgRev, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                JOIN Products p ON p.Id = f.ProductId
                LEFT JOIN AvgMonthly a ON a.ProductId = f.ProductId
                WHERE p.CompanyId = @CompanyId
                AND f.PeriodType = 'Monthly'
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateForecastsProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateForecasts stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateForecasts
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.UpdateDailyForecasts @CompanyId, @ProductIds;
                EXEC dbo.UpdateWeeklyForecasts @CompanyId, @ProductIds;
                EXEC dbo.UpdateMonthlyForecasts @CompanyId, @ProductIds;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
