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
                    SELECT 'Daily' AS PeriodType, DATEADD(DAY, -v.number, CAST(GETUTCDATE() AS DATE)) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number < 30
                )
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                SELECT p.Id, pr.PeriodType, pr.PeriodStartDate, pr.PeriodStartDate, 0, 0
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
                    SELECT 'Weekly' AS PeriodType, DATEADD(WEEK, -v.number, DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETUTCDATE() AS DATE)), CAST(GETUTCDATE() AS DATE))) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number < 13
                )
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                SELECT p.Id, pr.PeriodType, pr.PeriodStartDate, DATEADD(DAY, 6, pr.PeriodStartDate), 0, 0
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
                    SELECT 'Monthly' AS PeriodType, DATEFROMPARTS(YEAR(DATEADD(MONTH, -v.number, GETUTCDATE())), MONTH(DATEADD(MONTH, -v.number, GETUTCDATE())), 1) AS PeriodStartDate
                    FROM master.dbo.spt_values v
                    WHERE v.type = 'P' AND v.number < 12
                )
                INSERT INTO SalesAggregates (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                SELECT p.Id, pr.PeriodType, pr.PeriodStartDate, EOMONTH(pr.PeriodStartDate), 0, 0
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

                WITH DailyAgg AS (
                    SELECT 
                        p.Id,
                        CAST(t.TransactionDate AS DATE) AS PeriodStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    INNER JOIN Products p ON p.Id = tli.ProductId
                    WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
                    GROUP BY p.Id, CAST(t.TransactionDate AS DATE)
                )

                UPDATE s
                SET s.TotalQuantity = d.TotalQuantity,
                    s.TotalRevenue = d.TotalRevenue
                FROM SalesAggregates s
                JOIN DailyAgg d
                ON s.ProductId = d.Id
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

                WITH WeeklyAgg AS (
                    SELECT 
                        p.Id,
                        DATEADD(DAY, 1 - DATEPART(WEEKDAY, t.TransactionDate), CAST(t.TransactionDate AS DATE)) AS PeriodStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    INNER JOIN Products p ON p.Id = tli.ProductId
                    WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
                    GROUP BY p.Id, DATEADD(DAY, 1 - DATEPART(WEEKDAY, t.TransactionDate), CAST(t.TransactionDate AS DATE))
                )

                UPDATE s
                SET s.TotalQuantity = w.TotalQuantity,
                    s.TotalRevenue = w.TotalRevenue
                FROM SalesAggregates s
                JOIN WeeklyAgg w
                ON s.ProductId = w.Id
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

                WITH MonthlyAgg AS (
                    SELECT 
                        p.Id,
                        DATEFROMPARTS(YEAR(t.TransactionDate), MONTH(t.TransactionDate), 1) AS PeriodStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    INNER JOIN Products p ON p.Id = tli.ProductId
                    WHERE p.CompanyId = @CompanyId
                    AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                    )
                    GROUP BY p.Id, DATEFROMPARTS(YEAR(t.TransactionDate), MONTH(t.TransactionDate), 1)
                )

                UPDATE s
                SET s.TotalQuantity = m.TotalQuantity,
                    s.TotalRevenue = m.TotalRevenue
                FROM SalesAggregates s
                JOIN MonthlyAgg m
                ON s.ProductId = m.Id
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
