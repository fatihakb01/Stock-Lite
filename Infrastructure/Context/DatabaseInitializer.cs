using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            // Apply any pending migrations first
            await context.Database.MigrateAsync();

            // Ensure guid list table exist
            await EnsureGuidListTableAsync(context, logger);

            // Ensure stored procedures exist
            await EnsureStoredProceduresAsync(context, logger);
        }

        private static async Task EnsureGuidListTableAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring guid list table exist...");

            var sql = """
            -- Ensure the GuidList table type exists
            IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'GuidList')
            CREATE TYPE dbo.GuidList AS TABLE (Id UNIQUEIDENTIFIER);
            """;

            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private static async Task EnsureStoredProceduresAsync(DataContext context, ILogger logger)
        {
            logger.LogInformation("Ensuring stored procedures exist...");

            var sql = """
            -- Create the stored procedure if it doesn't exist
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'UpdateSalesAggregates')
            EXEC('
            CREATE PROCEDURE dbo.UpdateSalesAggregates
                @CompanyId UNIQUEIDENTIFIER,
                @ProductIds dbo.GuidList READONLY
            AS
            BEGIN
                SET NOCOUNT ON;

                --------------------------------------------------------
                -- DAILY AGGREGATION
                --------------------------------------------------------
                ;WITH DailyAgg AS (
                    SELECT 
                        tli.ProductId,
                        CAST(t.TransactionDate AS DATE) AS PeriodStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    WHERE t.CompanyId = @CompanyId
                    AND (@ProductIds IS NULL OR tli.ProductId IN (SELECT Id FROM @ProductIds))
                    GROUP BY tli.ProductId, CAST(t.TransactionDate AS DATE)
                )
                MERGE INTO SalesAggregates AS Target
                USING DailyAgg AS Source
                ON Target.ProductId = Source.ProductId
                AND Target.PeriodType = ''Daily''
                AND Target.PeriodStartDate = Source.PeriodStartDate
                WHEN MATCHED THEN
                    UPDATE SET 
                        Target.TotalQuantity = Source.TotalQuantity,
                        Target.TotalRevenue = Source.TotalRevenue
                WHEN NOT MATCHED THEN
                    INSERT (Id, ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                    VALUES (NEWID(), Source.ProductId, ''Daily'', Source.PeriodStartDate, Source.PeriodStartDate, Source.TotalQuantity, Source.TotalRevenue);

                --------------------------------------------------------
                -- WEEKLY AGGREGATION
                --------------------------------------------------------
                ;WITH WeeklyAgg AS (
                    SELECT 
                        tli.ProductId,
                        DATEADD(DAY, 1 - DATEPART(WEEKDAY, t.TransactionDate), CAST(t.TransactionDate AS DATE)) AS WeekStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    WHERE t.CompanyId = @CompanyId
                    AND (@ProductIds IS NULL OR tli.ProductId IN (SELECT Id FROM @ProductIds))
                    GROUP BY tli.ProductId, DATEADD(DAY, 1 - DATEPART(WEEKDAY, t.TransactionDate), CAST(t.TransactionDate AS DATE))
                )
                MERGE INTO SalesAggregates AS Target
                USING WeeklyAgg AS Source
                ON Target.ProductId = Source.ProductId
                AND Target.PeriodType = ''Weekly''
                AND Target.PeriodStartDate = Source.WeekStartDate
                WHEN MATCHED THEN
                    UPDATE SET 
                        Target.TotalQuantity = Source.TotalQuantity,
                        Target.TotalRevenue = Source.TotalRevenue
                WHEN NOT MATCHED THEN
                    INSERT (Id, ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                    VALUES (NEWID(), Source.ProductId, ''Weekly'', Source.WeekStartDate, DATEADD(DAY, 6, Source.WeekStartDate), Source.TotalQuantity, Source.TotalRevenue);

                --------------------------------------------------------
                -- MONTHLY AGGREGATION
                --------------------------------------------------------
                ;WITH MonthlyAgg AS (
                    SELECT 
                        tli.ProductId,
                        DATEFROMPARTS(YEAR(t.TransactionDate), MONTH(t.TransactionDate), 1) AS MonthStartDate,
                        SUM(tli.Quantity) AS TotalQuantity,
                        SUM(tli.Quantity * tli.UnitPrice) AS TotalRevenue
                    FROM TransactionLineItems tli
                    INNER JOIN Transactions t ON t.Id = tli.TransactionId
                    WHERE t.CompanyId = @CompanyId
                    AND (@ProductIds IS NULL OR tli.ProductId IN (SELECT Id FROM @ProductIds))
                    GROUP BY tli.ProductId, DATEFROMPARTS(YEAR(t.TransactionDate), MONTH(t.TransactionDate), 1)
                )
                MERGE INTO SalesAggregates AS Target
                USING MonthlyAgg AS Source
                ON Target.ProductId = Source.ProductId
                AND Target.PeriodType = ''Monthly''
                AND Target.PeriodStartDate = Source.MonthStartDate
                WHEN MATCHED THEN
                    UPDATE SET 
                        Target.TotalQuantity = Source.TotalQuantity,
                        Target.TotalRevenue = Source.TotalRevenue
                WHEN NOT MATCHED THEN
                    INSERT (Id, ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
                    VALUES (NEWID(), Source.ProductId, ''Monthly'', Source.MonthStartDate, EOMONTH(Source.MonthStartDate), Source.TotalQuantity, Source.TotalRevenue);

            END
            ');
                
            """;

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
