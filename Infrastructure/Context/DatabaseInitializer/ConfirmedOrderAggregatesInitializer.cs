using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class ConfirmedOrderAggregatesInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureDeletePastProc(context, logger);

        await InsertDailyAsync(context, logger);
        await InsertWeeklyAsync(context, logger);
        await InsertMonthlyAsync(context, logger);
        await InsertAllAsync(context, logger);

        await UpdateDailyAsync(context, logger);
        await UpdateWeeklyAsync(context, logger);
        await UpdateMonthlyAsync(context, logger);
        await UpdateAllAsync(context, logger);
    }

    // ---------------- DELETE PAST ----------------

    private static async Task EnsureDeletePastProc(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.DeletePastConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.DeletePastConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY,
            @PeriodType NVARCHAR(10)
        AS
        BEGIN
            SET NOCOUNT ON;

            DELETE coa
            FROM ConfirmedOrderAggregates coa
            JOIN Products p ON p.Id = coa.ProductId
            WHERE p.CompanyId = @CompanyId
              AND coa.PeriodType = @PeriodType
              AND coa.PeriodEndDate < CAST(SYSUTCDATETIME() AS DATE)
              AND (
                  NOT EXISTS (SELECT 1 FROM @ProductIds)
                  OR p.Id IN (SELECT Id FROM @ProductIds)
              );
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    // ---------------- INSERT ----------------

    private static async Task InsertDailyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring InsertDailyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertDailyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.DeletePastConfirmedOrderAggregates @CompanyId, @ProductIds, 'Daily';

            WITH Periods AS (
                SELECT 'Daily' AS PeriodType,
                       DATEADD(DAY, v.number, CAST(SYSUTCDATETIME() AS DATE)) AS PeriodStartDate
                FROM master.dbo.spt_values v
                WHERE v.type = 'P'
                  AND v.number BETWEEN 0 AND 29
            )
            INSERT INTO ConfirmedOrderAggregates
            (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalConfirmedQuantity, TotalConfirmedRevenue)
            SELECT
                p.Id,
                pr.PeriodType,
                pr.PeriodStartDate,
                pr.PeriodStartDate,
                0,
                0
            FROM Products p
            CROSS JOIN Periods pr
            LEFT JOIN ConfirmedOrderAggregates coa
              ON coa.ProductId = p.Id
             AND coa.PeriodType = pr.PeriodType
             AND coa.PeriodStartDate = pr.PeriodStartDate
            WHERE p.CompanyId = @CompanyId
              AND (
                  NOT EXISTS (SELECT 1 FROM @ProductIds)
                  OR p.Id IN (SELECT Id FROM @ProductIds)
              )
              AND coa.ProductId IS NULL;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task InsertWeeklyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring InsertWeeklyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertWeeklyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.DeletePastConfirmedOrderAggregates @CompanyId, @ProductIds, 'Weekly';

            WITH Periods AS (
                SELECT
                    'Weekly' AS PeriodType,
                    DATEADD(
                        WEEK,
                        v.number,
                        DATEADD(
                            DAY,
                            -((DATEDIFF(DAY, '19000101', CAST(SYSUTCDATETIME() AS DATE)) % 7)),
                            CAST(SYSUTCDATETIME() AS DATE)
                        )
                    ) AS PeriodStartDate
                FROM master.dbo.spt_values v
                WHERE v.type = 'P'
                  AND v.number BETWEEN 0 AND 12
            )
            INSERT INTO ConfirmedOrderAggregates
            (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalConfirmedQuantity, TotalConfirmedRevenue)
            SELECT
                p.Id,
                pr.PeriodType,
                pr.PeriodStartDate,
                DATEADD(DAY, 6, pr.PeriodStartDate),
                0,
                0
            FROM Products p
            CROSS JOIN Periods pr
            LEFT JOIN ConfirmedOrderAggregates coa
              ON coa.ProductId = p.Id
             AND coa.PeriodType = pr.PeriodType
             AND coa.PeriodStartDate = pr.PeriodStartDate
            WHERE p.CompanyId = @CompanyId
              AND (
                  NOT EXISTS (SELECT 1 FROM @ProductIds)
                  OR p.Id IN (SELECT Id FROM @ProductIds)
              )
              AND coa.ProductId IS NULL;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task InsertMonthlyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring InsertMonthlyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertMonthlyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.DeletePastConfirmedOrderAggregates @CompanyId, @ProductIds, 'Monthly';

            WITH Periods AS (
                SELECT
                    'Monthly' AS PeriodType,
                    DATEFROMPARTS(
                        YEAR(DATEADD(MONTH, v.number, SYSUTCDATETIME())),
                        MONTH(DATEADD(MONTH, v.number, SYSUTCDATETIME())),
                        1
                    ) AS PeriodStartDate
                FROM master.dbo.spt_values v
                WHERE v.type = 'P'
                  AND v.number BETWEEN 0 AND 11
            )
            INSERT INTO ConfirmedOrderAggregates
            (ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalConfirmedQuantity, TotalConfirmedRevenue)
            SELECT
                p.Id,
                pr.PeriodType,
                pr.PeriodStartDate,
                EOMONTH(pr.PeriodStartDate),
                0,
                0
            FROM Products p
            CROSS JOIN Periods pr
            LEFT JOIN ConfirmedOrderAggregates coa
              ON coa.ProductId = p.Id
             AND coa.PeriodType = pr.PeriodType
             AND coa.PeriodStartDate = pr.PeriodStartDate
            WHERE p.CompanyId = @CompanyId
              AND (
                  NOT EXISTS (SELECT 1 FROM @ProductIds)
                  OR p.Id IN (SELECT Id FROM @ProductIds)
              )
              AND coa.ProductId IS NULL;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task InsertAllAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring InsertConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.InsertMonthlyConfirmedOrderAggregates @CompanyId, @ProductIds;
            EXEC dbo.InsertWeeklyConfirmedOrderAggregates @CompanyId, @ProductIds;
            EXEC dbo.InsertDailyConfirmedOrderAggregates @CompanyId, @ProductIds;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    // ---------------- UPDATE ----------------

    private static async Task UpdateDailyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring UpdateDailyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateDailyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = 0,
                coa.TotalConfirmedRevenue = 0
            FROM ConfirmedOrderAggregates coa
            JOIN Products p ON p.Id = coa.ProductId
            WHERE p.CompanyId = @CompanyId
            AND coa.PeriodType = 'Daily'
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );

            WITH Agg AS (
                SELECT
                    coli.ProductId,
                    CAST(co.ExpectedDeliveryDate AS DATE) AS PeriodStartDate,
                    SUM(coli.Quantity - coli.SoldQuantity) AS Qty,
                    SUM((coli.Quantity - coli.SoldQuantity) * coli.UnitCost) AS Revenue
                FROM ConfirmedOrderLineItem coli
                JOIN ConfirmedOrder co ON co.Id = coli.ConfirmedOrderId
                WHERE co.CompanyId = @CompanyId
                  AND co.Status IN (0, 1)
                  AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR coli.ProductId IN (SELECT Id FROM @ProductIds)
                  )
                GROUP BY coli.ProductId, CAST(co.ExpectedDeliveryDate AS DATE)
            )
            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = a.Qty,
                coa.TotalConfirmedRevenue = a.Revenue
            FROM ConfirmedOrderAggregates coa
            JOIN Agg a
              ON coa.ProductId = a.ProductId
             AND coa.PeriodType = 'Daily'
             AND coa.PeriodStartDate = a.PeriodStartDate;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task UpdateWeeklyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring UpdateWeeklyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateWeeklyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = 0,
                coa.TotalConfirmedRevenue = 0
            FROM ConfirmedOrderAggregates coa
            JOIN Products p ON p.Id = coa.ProductId
            WHERE p.CompanyId = @CompanyId
            AND coa.PeriodType = 'Weekly'
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );

            WITH Agg AS (
                SELECT
                    coli.ProductId,
                    DATEADD(
                        DAY,
                        -((DATEDIFF(DAY, '19000101', CAST(co.ExpectedDeliveryDate AS DATE)) % 7)),
                        CAST(co.ExpectedDeliveryDate AS DATE)
                    ) AS PeriodStartDate,
                    SUM(coli.Quantity - coli.SoldQuantity) AS Qty,
                    SUM((coli.Quantity - coli.SoldQuantity) * coli.UnitCost) AS Revenue
                FROM ConfirmedOrderLineItem coli
                JOIN ConfirmedOrder co ON co.Id = coli.ConfirmedOrderId
                WHERE co.CompanyId = @CompanyId
                  AND co.Status IN (0, 1)
                  AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR coli.ProductId IN (SELECT Id FROM @ProductIds)
                  )
                GROUP BY
                    coli.ProductId,
                    DATEADD(
                        DAY,
                        -((DATEDIFF(DAY, '19000101', CAST(co.ExpectedDeliveryDate AS DATE)) % 7)),
                        CAST(co.ExpectedDeliveryDate AS DATE)
                    )
            )
            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = a.Qty,
                coa.TotalConfirmedRevenue = a.Revenue
            FROM ConfirmedOrderAggregates coa
            JOIN Agg a
              ON coa.ProductId = a.ProductId
             AND coa.PeriodType = 'Weekly'
             AND coa.PeriodStartDate = a.PeriodStartDate;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task UpdateMonthlyAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring UpdateMonthlyConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateMonthlyConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = 0,
                coa.TotalConfirmedRevenue = 0
            FROM ConfirmedOrderAggregates coa
            JOIN Products p ON p.Id = coa.ProductId
            WHERE p.CompanyId = @CompanyId
            AND coa.PeriodType = 'Monthly'
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );

            WITH Agg AS (
                SELECT
                    coli.ProductId,
                    DATEFROMPARTS(
                        YEAR(co.ExpectedDeliveryDate),
                        MONTH(co.ExpectedDeliveryDate),
                        1
                    ) AS PeriodStartDate,
                    SUM(coli.Quantity - coli.SoldQuantity) AS Qty,
                    SUM((coli.Quantity - coli.SoldQuantity) * coli.UnitCost) AS Revenue
                FROM ConfirmedOrderLineItem coli
                JOIN ConfirmedOrder co ON co.Id = coli.ConfirmedOrderId
                WHERE co.CompanyId = @CompanyId
                  AND co.Status IN (0, 1)
                  AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR coli.ProductId IN (SELECT Id FROM @ProductIds)
                  )
                GROUP BY
                    coli.ProductId,
                    DATEFROMPARTS(
                        YEAR(co.ExpectedDeliveryDate),
                        MONTH(co.ExpectedDeliveryDate),
                        1
                    )
            )
            UPDATE coa
            SET
                coa.TotalConfirmedQuantity = a.Qty,
                coa.TotalConfirmedRevenue = a.Revenue
            FROM ConfirmedOrderAggregates coa
            JOIN Agg a
              ON coa.ProductId = a.ProductId
             AND coa.PeriodType = 'Monthly'
             AND coa.PeriodStartDate = a.PeriodStartDate;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task UpdateAllAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring UpdateConfirmedOrderAggregates exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateConfirmedOrderAggregates
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.UpdateMonthlyConfirmedOrderAggregates @CompanyId, @ProductIds;
            EXEC dbo.UpdateWeeklyConfirmedOrderAggregates @CompanyId, @ProductIds;
            EXEC dbo.UpdateDailyConfirmedOrderAggregates @CompanyId, @ProductIds;
        END";
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
