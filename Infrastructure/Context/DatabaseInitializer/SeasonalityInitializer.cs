using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class SeasonalityInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await DeletePastSeasonalityAsync(context, logger);
        await InsertSeasonalityProcAsync(context, logger);
        await UpdateSeasonalityAsync(context, logger);
    }

    public static async Task DeletePastSeasonalityAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.DeletePastSeasonality stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.DeletePastSeasonality
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            DELETE s
            FROM Seasonality s
            JOIN Products p ON p.Id = s.ProductId
            WHERE p.CompanyId = @CompanyId
              AND EOMONTH(s.PeriodStartDate) < CONVERT(DATE, SYSUTCDATETIME())
              AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
              )
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task InsertSeasonalityProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.InsertSeasonality stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertSeasonality 
        @CompanyId UNIQUEIDENTIFIER,
        @ProductIds dbo.GuidList READONLY
        AS
        BEGIN

            WITH FutureMonths AS (
                SELECT DISTINCT
                    v.number AS Offset,
                    DATEFROMPARTS(YEAR(DATEADD(MONTH, v.number, DATEADD(MONTH, 0, CONVERT(DATE, SYSUTCDATETIME())))),
                                MONTH(DATEADD(MONTH, v.number, DATEADD(MONTH, 0, CONVERT(DATE, SYSUTCDATETIME())))), 1) AS PeriodStartDate
                FROM master.dbo.spt_values v
                WHERE v.number BETWEEN -1 AND 11
            ),
            ProductMonths AS (
                SELECT p.Id AS ProductId, fd.PeriodStartDate AS MonthStart
                FROM Products p
                CROSS JOIN FutureMonths fd
                WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )
            ) 
            INSERT INTO Seasonality (
                ProductId,
                PeriodStartDate,
                Factor
            )
            SELECT
                m.ProductId,
                m.MonthStart,
                1
            FROM ProductMonths m
            WHERE NOT EXISTS (
                SELECT 1
                FROM Seasonality s
                WHERE s.ProductId = m.ProductId
                AND s.PeriodStartDate = m.MonthStart
            )
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task UpdateSeasonalityAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.UpdateSeasonality stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateSeasonality
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            WITH MonthlySales AS (
                SELECT
                    sa.ProductId,
                    sa.PeriodStartDate,
                    MONTH(sa.PeriodStartDate) AS MonthNumber,
                    sa.TotalQuantity
                FROM SalesAggregates sa
                JOIN Products p ON p.Id = sa.ProductId
                WHERE sa.PeriodType = 'Monthly'
                AND p.CompanyId = @CompanyId
                AND (
                        NOT EXISTS (SELECT 1 FROM @ProductIds)
                        OR p.Id IN (SELECT Id FROM @ProductIds)
                )
            ),
            ProductCounts AS (
                SELECT
                    ProductId,
                    COUNT(*) AS MonthCount
                FROM MonthlySales
                GROUP BY ProductId
            ),
            OverallAverage AS (
                SELECT
                    ProductId,
                    AVG(CAST(TotalQuantity AS FLOAT)) AS OverallAvg
                FROM MonthlySales
                GROUP BY ProductId
            ),
            MonthAverage AS (
                SELECT
                    ProductId,
                    MonthNumber,
                    AVG(CAST(TotalQuantity AS FLOAT)) AS MonthAvg
                FROM MonthlySales
                GROUP BY ProductId, MonthNumber
            ) 

            UPDATE s
            SET s.Factor =
                CASE
                    WHEN pc.MonthCount < 12 THEN 1
                    WHEN oa.OverallAvg = 0 THEN 1
                    ELSE ma.MonthAvg / oa.OverallAvg
                END
            FROM Seasonality s
            JOIN Products p ON p.Id = s.ProductId
            JOIN ProductCounts pc ON pc.ProductId = s.ProductId
            JOIN OverallAverage oa ON oa.ProductId = s.ProductId
            JOIN MonthAverage ma
                ON ma.ProductId = s.ProductId
                AND ma.MonthNumber = MONTH(s.PeriodStartDate)

        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
