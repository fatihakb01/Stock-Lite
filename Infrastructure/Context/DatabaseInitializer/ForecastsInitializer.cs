using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer
{
    public static class ForecastsInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            // Delete forecasts in the past (per period)
            await DeletePastForecastsProcAsync(context, logger);

            // Insert forecasts where no forecast exists
            await InsertMonthlyForecastsProcAsync(context, logger);
            await InsertWeeklyForecastsProcAsync(context, logger);
            await InsertDailyForecastsProcAsync(context, logger);
            await InsertForecastsProcAsync(context, logger);

            // Update forecasts that already exist
            await UpdateMonthlyForecastsProcAsync(context, logger);
            await UpdateWeeklyForecastsProcAsync(context, logger);
            await UpdateDailyForecastsProcAsync(context, logger);
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
                  AND f.PeriodType = @PeriodType
                  AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                  )
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

                -- Clean up past monthly forecasts for the provided products
                EXEC dbo.DeletePastForecasts @CompanyId, @ProductIds, 'Monthly';
				
				-- Compute average monthly quantity/revenue from SalesAggregates
                WITH FutureMonths AS (
                    -- Next 12 months starting with this coming month
                    SELECT 
                        v.number AS Offset,
                        DATEFROMPARTS(YEAR(DATEADD(MONTH, v.number, DATEADD(MONTH, 0, CONVERT(DATE, SYSUTCDATETIME())))),
                                      MONTH(DATEADD(MONTH, v.number, DATEADD(MONTH, 0, CONVERT(DATE, SYSUTCDATETIME())))), 1) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number BETWEEN 0 AND 11
                ),
                ProductMonths AS (
                    SELECT p.Id AS ProductId, fd.PeriodStartDate AS MonthStart, EOMONTH(fd.PeriodStartDate) AS MonthEnd
                    FROM Products p
                    CROSS JOIN FutureMonths fd
                    WHERE p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ) 
                INSERT INTO Forecasts (
					ProductId, 
					PeriodType, 
					PeriodStartDate, 
					PeriodEndDate, 
					PredictedQuantity, 
					PredictedRevenue, 
					Confidence
				)
                SELECT
                    pm.ProductId,
                    'Monthly',
                    pm.MonthStart,
                    pm.MonthEnd,
                    0,
                    0,
                    0.50
                FROM ProductMonths pm
                WHERE NOT EXISTS (
					SELECT 1
					FROM Forecasts f
					WHERE f.ProductId = pm.ProductId
					AND f.PeriodType = 'Monthly'
					AND f.PeriodStartDate = pm.MonthStart
				)
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

                WITH BaseMonday AS (
					SELECT
						DATEADD(
							DAY,
							-((DATEPART(WEEKDAY, CONVERT(DATE, SYSUTCDATETIME())) + @@DATEFIRST - 2) % 7),
							CONVERT(DATE, SYSUTCDATETIME())
						) AS MondayStart
				),
				FutureWeeks AS (
					SELECT
						v.number AS WeekOffset,
						DATEADD(WEEK, v.number, bm.MondayStart) AS WeekStart,
						DATEADD(DAY, 6, DATEADD(WEEK, v.number, bm.MondayStart)) AS WeekEnd
					FROM master.dbo.spt_values v
					CROSS JOIN BaseMonday bm
					WHERE v.type = 'P' AND v.number BETWEEN 0 AND 51 
				),
                ProductWeeks AS (
                    SELECT p.Id AS ProductId, fw.WeekStart, fw.WeekEnd
                    FROM Products p
                    CROSS JOIN FutureWeeks fw
                    WHERE p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                )

                INSERT INTO Forecasts (
					ProductId, 
					PeriodType, 
					PeriodStartDate, 
					PeriodEndDate, 
					PredictedQuantity, 
					PredictedRevenue, 
					Confidence
				)
                SELECT
                    pw.ProductId,
                    'Weekly',
                    pw.WeekStart,
                    pw.WeekEnd,
                    0,
                    0,
                    0.50
                FROM ProductWeeks pw
                WHERE NOT EXISTS (
					SELECT 1
					FROM Forecasts f
					WHERE f.ProductId = pw.ProductId
					AND f.PeriodType = 'Weekly'
					AND f.PeriodStartDate = pw.WeekStart
				)
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
				
				WITH FutureDays AS (
                    SELECT v.number AS Offset,
                           DATEADD(DAY, v.number, CONVERT(DATE, SYSUTCDATETIME())) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number BETWEEN 0 AND 29
				),
                ProductDays AS (
                    SELECT p.Id AS ProductId, fd.PeriodStartDate AS DayStart, fd.PeriodStartDate AS DayEnd
                    FROM Products p
                    CROSS JOIN FutureDays fd
                    WHERE p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ) 
                INSERT INTO Forecasts (
					ProductId, 
					PeriodType, 
					PeriodStartDate, 
					PeriodEndDate, 
					PredictedQuantity, 
					PredictedRevenue, 
					Confidence
				)
                SELECT
                    pd.ProductId,
                    'Daily',
                    pd.DayStart,
                    pd.DayEnd,
                    0,
                    0,
                    0.50
                FROM ProductDays pd
				WHERE NOT EXISTS (
					SELECT 1
					FROM Forecasts f
					WHERE f.ProductId = pd.ProductId
					AND f.PeriodType = 'Daily'
					AND f.PeriodStartDate = pd.DayStart
				)
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

                EXEC dbo.InsertMonthlyForecasts @CompanyId, @ProductIds;
                EXEC dbo.InsertWeeklyForecasts @CompanyId, @ProductIds;
                EXEC dbo.InsertDailyForecasts @CompanyId, @ProductIds;
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

                WITH ForecastMonthly AS (
                    SELECT 
                        sa.ProductId,
                        AVG(CAST(sa.TotalQuantity AS FLOAT)) AS AvgQty,
                        AVG(CAST(sa.TotalRevenue AS FLOAT)) AS AvgRev
                    FROM SalesAggregates sa
                    JOIN Products p ON p.Id = sa.ProductId
                    WHERE sa.PeriodType = 'Monthly'
                      AND p.CompanyId = @CompanyId
                      AND sa.PeriodStartDate >= DATEADD(MONTH, -12, CONVERT(DATE, SYSUTCDATETIME()))
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                    GROUP BY sa.ProductId
                )
                UPDATE f
                SET f.PredictedQuantity = ISNULL(fm.AvgQty, 0),
                    f.PredictedRevenue = ISNULL(fm.AvgRev, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                LEFT JOIN ForecastMonthly fm 
					ON fm.ProductId = f.ProductId
				    AND f.PeriodType = 'Monthly'
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
				
				WITH ForecastMonthly AS (
					SELECT 
						ProductId,
						PredictedQuantity,
						PredictedRevenue,
						PeriodStartDate,
						PeriodEndDate
					FROM Forecasts f
					JOIN Products p ON p.Id = f.ProductId
					WHERE f.PeriodType = 'Monthly'
					  AND p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ),
                ForecastMonths AS (
					SELECT 
						DATEFROMPARTS(YEAR(DATEADD(MONTH, v.number, CONVERT(DATE, SYSUTCDATETIME()))),
									  MONTH(DATEADD(MONTH, v.number, CONVERT(DATE, SYSUTCDATETIME()))), 1) AS MonthStart,
						EOMONTH(DATEADD(MONTH, v.number, CONVERT(DATE, SYSUTCDATETIME()))) AS MonthEnd
					FROM master.dbo.spt_values v
					WHERE v.type = 'P' AND v.number BETWEEN 0 AND 11
                ),
				BaseMonday AS (
					SELECT
						DATEADD(
							DAY,
							-((DATEPART(WEEKDAY, CONVERT(DATE, SYSUTCDATETIME())) + @@DATEFIRST - 2) % 7),
							CONVERT(DATE, SYSUTCDATETIME())
						) AS MondayStart
				),
				FutureWeeks AS (
					SELECT
						v.number AS WeekOffset,
						DATEADD(WEEK, v.number, bm.MondayStart) AS WeekStart,
						DATEADD(DAY, 6, DATEADD(WEEK, v.number, bm.MondayStart)) AS WeekEnd
					FROM master.dbo.spt_values v
					CROSS JOIN BaseMonday bm
					WHERE v.type = 'P' AND v.number BETWEEN 0 AND 51 
				),
                ProductWeeks AS (
                    SELECT p.Id AS ProductId, fw.WeekStart, fw.WeekEnd
                    FROM Products p
                    CROSS JOIN FutureWeeks fw
                    WHERE p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ),
                WeekMonthOverlap AS (
                    SELECT
                        pw.ProductId,
                        pw.WeekStart,
                        pw.WeekEnd,
                        fm.MonthStart,
                        fm.MonthEnd,
                        CASE WHEN fm.MonthStart > pw.WeekStart THEN fm.MonthStart ELSE pw.WeekStart END AS OverlapStart,
                        CASE WHEN fm.MonthEnd < pw.WeekEnd THEN fm.MonthEnd ELSE pw.WeekEnd END AS OverlapEnd
                    FROM ProductWeeks pw
                    CROSS JOIN ForecastMonths fm
                    WHERE fm.MonthStart <= pw.WeekEnd
                      AND fm.MonthEnd >= pw.WeekStart
                ),
                WeekMonthWeighted AS (
                    SELECT
                        wmo.ProductId,
                        wmo.WeekStart AS PeriodStartDate,
                        wmo.WeekEnd AS PeriodEndDate,
                        CASE 
                            WHEN DATEDIFF(DAY, wmo.OverlapStart, wmo.OverlapEnd) < 0 THEN 0
                            ELSE DATEDIFF(DAY, wmo.OverlapStart, wmo.OverlapEnd) + 1
                        END AS OverlapDays,
                        fmCalc.PredictedQuantity,
                        fmCalc.PredictedRevenue
                    FROM WeekMonthOverlap wmo
                    JOIN ForecastMonthly fmCalc
                        ON fmCalc.ProductId IS NOT NULL 
						AND fmCalc.ProductId = wmo.ProductId
						AND fmCalc.PeriodStartDate = wmo.MonthStart
						AND fmCalc.PeriodEndDate = wmo.MonthEnd
                ),
                ForecastWeekly AS (
                    SELECT
                        wm.ProductId,
                        wm.PeriodStartDate,
                        wm.PeriodEndDate,
                        SUM(ISNULL(wm.PredictedQuantity, 0) * (CAST(wm.OverlapDays AS FLOAT) / 7.0) * (12.0/52.0)) AS PredictedQuantity,
                        SUM(ISNULL(wm.PredictedRevenue, 0) * (CAST(wm.OverlapDays AS FLOAT) / 7.0) * (12.0/52.0)) AS PredictedRevenue
                    FROM WeekMonthWeighted wm
                    GROUP BY wm.ProductId, wm.PeriodStartDate, wm.PeriodEndDate
                )
                UPDATE f
                SET f.PredictedQuantity = ISNULL(fw.PredictedQuantity, 0),
                    f.PredictedRevenue = ISNULL(fw.PredictedRevenue, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                JOIN ForecastWeekly fw
                    ON fw.ProductId = f.ProductId
                    AND fw.PeriodStartDate = f.PeriodStartDate
                    AND f.PeriodType = 'Weekly'
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

				WITH ForecastWeekly AS (
					SELECT 
						ProductId,
						PredictedQuantity,
						PredictedRevenue,
						PeriodStartDate,
						PeriodEndDate
					FROM Forecasts f
					JOIN Products p ON p.Id = f.ProductId
					WHERE f.PeriodStartDate >= DATEADD(WEEK, -13, CONVERT(DATE, SYSUTCDATETIME()))
                      AND f.PeriodType = 'Weekly'
					  AND p.CompanyId = @CompanyId
					  AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ),
				FutureDays AS (
                    SELECT v.number AS Offset,
                           DATEADD(DAY, v.number, CONVERT(DATE, SYSUTCDATETIME())) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number BETWEEN 0 AND 29
				),
                ProductDays AS (
                    SELECT p.Id AS ProductId, fd.PeriodStartDate AS DayStart, fd.PeriodStartDate AS DayEnd
                    FROM Products p
                    CROSS JOIN FutureDays fd
                    WHERE p.CompanyId = @CompanyId
                      AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                      )
                ),
				BaseMonday AS (
					SELECT
						DATEADD(
							DAY,
							-((DATEPART(WEEKDAY, CONVERT(DATE, SYSUTCDATETIME())) + @@DATEFIRST - 2) % 7),
							CONVERT(DATE, SYSUTCDATETIME())
						) AS MondayStart
				),
				ForecastWeeks AS (
					SELECT
						v.number AS WeekOffset,
						DATEADD(WEEK, v.number, bm.MondayStart) AS WeekStart,
						DATEADD(DAY, 6, DATEADD(WEEK, v.number, bm.MondayStart)) AS WeekEnd
					FROM master.dbo.spt_values v
					CROSS JOIN BaseMonday bm
					WHERE v.type = 'P' AND v.number BETWEEN 0 AND 12   
                ) ,
                DayWeekOverlap AS (
                    SELECT
                        pw.ProductId,
                        pw.DayStart,
                        pw.DayEnd,
                        fw.WeekStart,
                        fw.WeekEnd
                    FROM ProductDays pw
                    CROSS JOIN ForecastWeeks fw
                    WHERE fw.WeekStart <= pw.DayEnd
                      AND fw.WeekEnd >= pw.DayStart
                ),
                ForecastDaily AS (
                    SELECT
                        dwo.ProductId,
                        dwo.DayStart AS PeriodStartDate,
                        dwo.DayEnd AS PeriodEndDate,
                        ISNULL(fwCalc.PredictedQuantity, 0) / 7.0 AS PredictedQuantity,
                        ISNULL(fwCalc.PredictedRevenue, 0) / 7.0 AS PredictedRevenue
                    FROM DayWeekOverlap dwo
                    JOIN ForecastWeekly fwCalc
                        ON fwCalc.ProductId IS NOT NULL 
						AND fwCalc.ProductId = dwo.ProductId
						AND fwCalc.PeriodStartDate = dwo.WeekStart
						AND fwCalc.PeriodEndDate = dwo.WeekEnd
                )
				
                UPDATE f
                SET f.PredictedQuantity = ISNULL(fd.PredictedQuantity, 0),
                    f.PredictedRevenue = ISNULL(fd.PredictedRevenue, 0),
                    f.Confidence = 0.50
                FROM Forecasts f
                JOIN ForecastDaily fd
                    ON fd.ProductId = f.ProductId
                    AND fd.PeriodStartDate = f.PeriodStartDate
                    AND f.PeriodType = 'Daily'
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

                EXEC dbo.UpdateMonthlyForecasts @CompanyId, @ProductIds;
                EXEC dbo.UpdateWeeklyForecasts @CompanyId, @ProductIds;
                EXEC dbo.UpdateDailyForecasts @CompanyId, @ProductIds;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
