using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer
{
    public static class SalesAggregatesInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            // Initialize empty sales aggregates
            await InsertDailySalesAggregatesProcAsync(context, logger);
            await InsertWeeklySalesAggregatesProcAsync(context, logger);
            await InsertMonthlySalesAggregatesProcAsync(context, logger);
            await InsertSalesAggregatesProcAsync(context, logger);

            // Update sales aggregates
            await UpdateDailySalesAggregatesProcAsync(context, logger);
            await UpdateWeeklySalesAggregatesProcAsync(context, logger);
            await UpdateMonthlySalesAggregatesProcAsync(context, logger);
            await UpdateSalesAggregatesProcAsync(context, logger);
        }

        private static async Task InsertDailySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertDailySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertDailySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                WITH Periods AS (
                    SELECT 'Daily' AS PeriodType, DATEADD(DAY, -v.number, CAST(SYSUTCDATETIME() AS DATE)) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' 
                    AND v.number BETWEEN 1 AND 30
                )
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                SELECT 
                    p.Id, 
                    pr.PeriodType, 
                    pr.PeriodStartDate, 
                    pr.PeriodStartDate, 
                    0, 
                    0
                FROM Products p
                CROSS JOIN Periods pr
                LEFT JOIN SalesAggregates sa
                ON sa.ProductId = p.Id
                AND sa.PeriodType = pr.PeriodType
                AND sa.PeriodStartDate = pr.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )
                AND sa.ProductId IS NULL;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertWeeklySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertWeeklySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertWeeklySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

				WITH Periods AS (
					SELECT
						'Weekly' AS PeriodType,
						DATEADD(
							WEEK,
							-v.number,
							DATEADD(
								DAY,
								-((DATEDIFF(DAY, '19000101', CAST(SYSUTCDATETIME() AS DATE)) % 7)),
								CAST(SYSUTCDATETIME() AS DATE)
							)
						) AS PeriodStartDate
					FROM master.dbo.spt_values v
					WHERE v.type = 'P'
					  AND v.number BETWEEN 1 AND 13
				)
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
				SELECT
					p.Id,
					pr.PeriodType,
					pr.PeriodStartDate,
					DATEADD(DAY, 6, pr.PeriodStartDate) AS PeriodEndDate,
					0 AS TotalQuantity,
					0 AS TotalRevenue
                FROM Products p
                CROSS JOIN Periods pr
                LEFT JOIN SalesAggregates sa
                ON sa.ProductId = p.Id
                AND sa.PeriodType = pr.PeriodType
                AND sa.PeriodStartDate = pr.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )
                AND sa.ProductId IS NULL;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertMonthlySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertMonthlySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertMonthlySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;
				
				WITH Periods AS (
                    SELECT 'Monthly' AS PeriodType, DATEFROMPARTS(YEAR(DATEADD(MONTH, -v.number, SYSUTCDATETIME())), MONTH(DATEADD(MONTH, -v.number, SYSUTCDATETIME())), 1) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' 
					AND v.number BETWEEN 1 AND 12
                )
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                SELECT 
					p.Id, 
					pr.PeriodType, 
					pr.PeriodStartDate, 
					EOMONTH(pr.PeriodStartDate), 
					0, 
					0
                FROM Products p
                CROSS JOIN Periods pr
                LEFT JOIN SalesAggregates sa
                ON sa.ProductId = p.Id
                AND sa.PeriodType = pr.PeriodType
                AND sa.PeriodStartDate = pr.PeriodStartDate
                WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )
                AND sa.ProductId IS NULL;
			END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task InsertSalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.InsertSalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.InsertSalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.InsertMonthlySalesAggregates @CompanyId, @ProductIds;
                EXEC dbo.InsertWeeklySalesAggregates @CompanyId, @ProductIds;
                EXEC dbo.InsertDailySalesAggregates @CompanyId, @ProductIds;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateDailySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateDailySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateDailySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                WITH Periods AS (
                    SELECT 'Daily' AS PeriodType, 
					DATEADD(DAY, -v.number, CAST(SYSUTCDATETIME() AS DATE)) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' 
                    AND v.number BETWEEN 1 AND 30
                ),		
				DailyProducts AS (
					SELECT 
						p.Id,
						pr.PeriodType,
						pr.PeriodStartDate
					FROM Periods pr
					JOIN SalesAggregates s
						ON s.PeriodType = pr.PeriodType
					   AND s.PeriodStartDate = pr.PeriodStartDate
					JOIN Products p 
						ON p.Id = s.ProductId
					WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
				),
				DailyAgg AS (
					SELECT 
						dp.Id AS ProductId,
						dp.PeriodStartDate,
						SUM(tli.Quantity) AS TotalQuantity,
						SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
					FROM DailyProducts dp
					JOIN TransactionLineItems tli
						ON tli.ProductId = dp.Id
					JOIN Transactions t
						ON t.Id = tli.TransactionId
					   AND CAST(t.TransactionDate AS DATE) = dp.PeriodStartDate
					GROUP BY 
						dp.Id,
						dp.PeriodStartDate
                )

                UPDATE s
                SET s.TotalQuantity = d.TotalQuantity,
                    s.TotalRevenue = d.TotalRevenue
                FROM SalesAggregates s
                JOIN DailyAgg d
                ON s.ProductId = d.ProductId
                AND s.PeriodType = 'Daily'
                AND s.PeriodStartDate = d.PeriodStartDate
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateWeeklySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateWeeklySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateWeeklySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

				WITH Periods AS (
					SELECT
						'Weekly' AS PeriodType,
						DATEADD(
							WEEK,
							-v.number,
							DATEADD(
								DAY,
								-((DATEDIFF(DAY, '19000101', CAST(SYSUTCDATETIME() AS DATE)) % 7)),
								CAST(SYSUTCDATETIME() AS DATE)
							)
						) AS PeriodStartDate
					FROM master.dbo.spt_values v
					WHERE v.type = 'P'
					  AND v.number BETWEEN 1 AND 13
				),
				WeeklyProducts AS (
					SELECT 
						p.Id,
						pr.PeriodType,
						pr.PeriodStartDate
					FROM Periods pr
					JOIN SalesAggregates s
						ON s.PeriodType = pr.PeriodType
					   AND s.PeriodStartDate = pr.PeriodStartDate
					JOIN Products p 
						ON p.Id = s.ProductId
					WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
				),
				WeeklyAgg AS (
					SELECT 
						wp.Id AS ProductId,
						wp.PeriodStartDate,
						SUM(tli.Quantity) AS TotalQuantity,
						SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
					FROM WeeklyProducts wp
					JOIN TransactionLineItems tli
						ON tli.ProductId = wp.Id
					JOIN Transactions t
						ON t.Id = tli.TransactionId
					   AND CAST(t.TransactionDate AS DATE) >= wp.PeriodStartDate
					   AND CAST(t.TransactionDate AS DATE) < DATEADD(DAY, 7, wp.PeriodStartDate)
					GROUP BY 
						wp.Id,
						wp.PeriodStartDate
				)

                UPDATE s
                SET s.TotalQuantity = w.TotalQuantity,
                    s.TotalRevenue = w.TotalRevenue
                FROM SalesAggregates s
                JOIN WeeklyAgg w
                ON s.ProductId = w.ProductId
                AND s.PeriodType = 'Weekly'
                AND s.PeriodStartDate = w.PeriodStartDate
			END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateMonthlySalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateMonthlySalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateMonthlySalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;
				
				WITH Periods AS (
                    SELECT 'Monthly' AS PeriodType, 
					DATEFROMPARTS(YEAR(DATEADD(MONTH, -v.number, SYSUTCDATETIME())), MONTH(DATEADD(MONTH, -v.number, SYSUTCDATETIME())), 1) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' 
					AND v.number BETWEEN 1 AND 12
                ),		
				MonthlyProducts AS (
					SELECT 
						p.Id,
						pr.PeriodType,
						pr.PeriodStartDate
					FROM Periods pr
					JOIN SalesAggregates s
						ON s.PeriodType = pr.PeriodType
					   AND s.PeriodStartDate = pr.PeriodStartDate
					JOIN Products p 
						ON p.Id = s.ProductId
					WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
				),
				MonthlyAgg AS (
					SELECT 
						mp.Id AS ProductId,
						mp.PeriodStartDate,
						SUM(tli.Quantity) AS TotalQuantity,
						SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
					FROM MonthlyProducts mp
					JOIN TransactionLineItems tli
						ON tli.ProductId = mp.Id
					JOIN Transactions t
						ON t.Id = tli.TransactionId
					   AND CAST(t.TransactionDate AS DATE) >= mp.PeriodStartDate
                       AND CAST(t.TransactionDate AS DATE) <= EOMONTH(mp.PeriodStartDate)
					GROUP BY 
						mp.Id,
						mp.PeriodStartDate
                )

				UPDATE s
                SET s.TotalQuantity = m.TotalQuantity,
                    s.TotalRevenue = m.TotalRevenue
                FROM SalesAggregates s
                JOIN MonthlyAgg m
                ON s.ProductId = m.ProductId
                AND s.PeriodType = 'Monthly'
                AND s.PeriodStartDate = m.PeriodStartDate
			END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task UpdateSalesAggregatesProcAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring dbo.UpdateSalesAggregates stored procedures exist...");

            var sql = @"
            CREATE OR ALTER PROCEDURE dbo.UpdateSalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                EXEC dbo.UpdateMonthlySalesAggregates @CompanyId, @ProductIds;
                EXEC dbo.UpdateWeeklySalesAggregates @CompanyId, @ProductIds;
                EXEC dbo.UpdateDailySalesAggregates @CompanyId, @ProductIds;
            END
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
