using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class StockLevelInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await CreatePfcWithinLeadTimeAgg(context, logger);
        await InsertProcAsync(context, logger);
        await UpdateProcAsync(context, logger);
    }

    private static async Task CreatePfcWithinLeadTimeAgg(DataContext context, ILogger logger)
    {
        var sql = @"
        CREATE OR ALTER VIEW [dbo].[PurchaseForecastWithinLeadTimeAgg]
        AS
        WITH ForecastWindow AS (
            SELECT
                pol.ProductId,
                CONVERT(date, SYSUTCDATETIME()) AS WindowStart,
                DATEADD(DAY, pol.LeadTimeDays, CONVERT(date, SYSUTCDATETIME())) AS WindowEnd
            FROM ProductInventoryPolicies pol
        ),
        WeightedForecast AS (
            SELECT
                pf.ProductId,
                pf.WeekIndex,
                pf.WeekStartDate,
                pf.WeekEndDate,
                pf.ProjectedStock,
                pf.RecommendedOrderQuantity,
                pf.QuantityCommitted,
                pf.QuantityOnOrder,

                -- Calculate overlapping days
                DATEDIFF(
                    DAY,
                    CASE
                        WHEN pf.WeekStartDate > fw.WindowStart THEN pf.WeekStartDate
                        ELSE fw.WindowStart
                    END,
                    CASE
                        WHEN pf.WeekEndDate < fw.WindowEnd THEN DATEADD(DAY, 1, pf.WeekEndDate)
                        ELSE fw.WindowEnd
                    END
                ) AS OverlapDays,

                pf.ForecastQuantity
            FROM PurchaseForecasts pf
            JOIN ForecastWindow fw
                ON fw.ProductId = pf.ProductId
            WHERE
                pf.WeekEndDate > fw.WindowStart
                AND pf.WeekStartDate < fw.WindowEnd
        ),
        RankedForecast AS (
            SELECT
                wf.ProductId,
                wf.WeekIndex,
                wf.ProjectedStock * (wf.OverlapDays / 7.0) AS WeightedProjectedStock,
                wf.RecommendedOrderQuantity,
                wf.QuantityCommitted * (wf.OverlapDays / 7.0) AS WeightedQuantityCommitted,
                wf.QuantityOnOrder * (wf.OverlapDays / 7.0) AS WeightedQuantityOnOrder,

                -- Weighted forecast
                wf.ForecastQuantity * (wf.OverlapDays / 7.0) AS WeightedForecastQuantity,

                -- Effective demand based on weighted forecast
                CASE
                    WHEN wf.ForecastQuantity * (wf.OverlapDays / 7.0) > wf.QuantityCommitted * (wf.OverlapDays / 7.0)
                    THEN wf.ForecastQuantity * (wf.OverlapDays / 7.0)
                    ELSE wf.QuantityCommitted * (wf.OverlapDays / 7.0)
                END AS EffectiveDemand,

                ROW_NUMBER() OVER (
                    PARTITION BY wf.ProductId
                    ORDER BY wf.WeekIndex DESC
                ) AS ProjectedStockRowNumber,

                ROW_NUMBER() OVER (
                    PARTITION BY wf.ProductId
                    ORDER BY wf.WeekIndex ASC
                ) AS RecommendedOrderQuantityRowNumber
            FROM WeightedForecast wf
        )

        SELECT
            ProductId,
            MAX(CASE WHEN RecommendedOrderQuantityRowNumber = 1 THEN RecommendedOrderQuantity END)	AS RecommendedOrderQuantity,
            SUM(WeightedQuantityCommitted)															AS QuantityCommitted,
            SUM(WeightedQuantityOnOrder)															AS QuantityOnOrder,
            SUM(WeightedForecastQuantity)															AS ForecastQuantity,
            SUM(EffectiveDemand)																	AS EffectiveDemand,
            MAX(CASE WHEN ProjectedStockRowNumber = 1 THEN WeightedProjectedStock END)				AS ProjectedStockAtLeadTime
        FROM RankedForecast
        GROUP BY ProductId;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task InsertProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.InsertStockLevels stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertStockLevels
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            INSERT INTO StockLevels (
                ProductId,
                QuantityOnHand,
                QuantityOnOrder,
                QuantityCommitted,
                NetStockPosition,
                ForecastQuantity,
                ReorderPoint,
                NeedsReorder,
                RecommendedOrderQuantity,
                LastCalculatedAt
            )
            SELECT
                p.Id,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                SYSUTCDATETIME()
            FROM Products p
            LEFT JOIN StockLevels sl ON sl.ProductId = p.Id
            WHERE sl.ProductId IS NULL
            AND p.CompanyId = @CompanyId
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task UpdateProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.UpdateStockLevels stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.UpdateStockLevels
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            UPDATE sl
            SET
                sl.QuantityOnHand = ISNULL(inv.QuantityOnHand, 0),
                sl.QuantityOnOrder = ISNULL(pfc.QuantityOnOrder, 0),
                sl.QuantityCommitted = ISNULL(pfc.QuantityCommitted, 0),

                sl.NetStockPosition =
                    ISNULL(inv.QuantityOnHand, 0)
                    + ISNULL(pfc.QuantityOnOrder, 0)
                    - ISNULL(pfc.EffectiveDemand, 0),

                sl.ForecastQuantity = 
                    ISNULL(pfc.ForecastQuantity, 0),

                sl.ReorderPoint =
                    pol.SafetyStock,

                sl.NeedsReorder =
                    CASE
                        WHEN ISNULL(pfc.ProjectedStockAtLeadTime, 0) < pol.SafetyStock
                        THEN 1 ELSE 0
                    END,

                sl.RecommendedOrderQuantity =
                    ISNULL(pfc.RecommendedOrderQuantity, 0),

                sl.LastCalculatedAt = SYSUTCDATETIME()
            FROM StockLevels sl
            JOIN Products p ON p.Id = sl.ProductId
            JOIN ProductInventoryPolicies pol ON pol.ProductId = p.Id
            LEFT JOIN InventorySnapshot inv ON inv.ProductId = p.Id
            LEFT JOIN PurchaseForecastWithinLeadTimeAgg pfc ON pfc.ProductId = p.Id
            WHERE p.CompanyId = @CompanyId
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
