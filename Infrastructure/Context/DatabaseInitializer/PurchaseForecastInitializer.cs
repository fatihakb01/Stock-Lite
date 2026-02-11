using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class PurchaseForecastInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await CreateWeeklyForecastAgg(context, logger);
        await CreateWeeklyCommittedAgg(context, logger);
        await CreateInventorySnapshot(context, logger);
        await CreateWeeklyOpenPurchaseOrdersAgg(context, logger);

        await DeleteProcAsync(context, logger);
        await InsertProcAsync(context, logger);
        await UpdateProcAsync(context, logger);
    }

    #region Views

    private static async Task CreateWeeklyForecastAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.WeeklyForecastAgg view exists...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.WeeklyForecastAgg AS
        WITH BaseMonday AS (
            SELECT
                DATEADD(
                    DAY,
                    -((DATEPART(WEEKDAY, CONVERT(date, SYSUTCDATETIME())) + @@DATEFIRST - 2) % 7),
                    CONVERT(date, SYSUTCDATETIME())
                ) AS MondayStart
        )
        SELECT
            f.ProductId,
            f.PeriodStartDate AS WeekStartDate,
            f.PeriodEndDate   AS WeekEndDate,
            f.PredictedQuantity AS ForecastQuantity,
            DATEDIFF(
                WEEK,
                bm.MondayStart,
                f.PeriodStartDate
            ) AS WeekIndex
        FROM Forecasts f
        CROSS JOIN BaseMonday bm
        WHERE f.PeriodType = 'Weekly'
        AND f.PeriodStartDate >= bm.MondayStart
        AND DATEDIFF(WEEK, bm.MondayStart, f.PeriodStartDate) BETWEEN 0 AND 51;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateWeeklyCommittedAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.WeeklyCommittedAgg view exists...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.WeeklyCommittedAgg AS
        SELECT
            col.ProductId,
            DATEADD(
                WEEK,
                DATEDIFF(WEEK, 0, co.ExpectedDeliveryDate),
                0
            ) AS WeekStartDate,
            SUM(col.Quantity - ISNULL(col.SoldQuantity, 0)) AS QuantityCommitted
        FROM ConfirmedOrderLineItem col
        JOIN ConfirmedOrder co ON co.Id = col.ConfirmedOrderId
        WHERE co.Status IN (0, 1)
        GROUP BY
            col.ProductId,
            DATEADD(WEEK, DATEDIFF(WEEK, 0, co.ExpectedDeliveryDate), 0);
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateInventorySnapshot(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.InventorySnapshot view exists...");

        var sql = @"
        CREATE OR ALTER VIEW [dbo].[InventorySnapshot] AS
		WITH AggregatedFilfilledPOs AS (
			SELECT
				p.Id AS ProductId,
				ISNULL(SUM(pol.ReceivedQuantity), 0) AS TotalReceivedQuantity
			FROM Products p
			JOIN PurchaseOrderLineItem pol ON pol.ProductId = p.Id
			JOIN PurchaseOrder po ON po.Id = pol.PurchaseOrderId AND po.Status IN (2, 3)
			GROUP BY p.Id
		),
		AggregatedTransactions AS (
			SELECT
				p.Id AS ProductId,
				ISNULL(SUM(tli.Quantity), 0) AS TotalSoldQuantity
			FROM Products p
			JOIN TransactionLineItems tli ON p.Id = tli.ProductId
			GROUP BY p.Id
		),
		Result AS (
			SELECT 
				t.ProductId AS ProductId,
				ISNULL(TotalReceivedQuantity, 0) - ISNULL(TotalSoldQuantity,0) AS QuantityOnHand
			FROM Products p
			LEFT JOIN AggregatedFilfilledPOs po ON p.Id = po.ProductId
			LEFT JOIN AggregatedTransactions t ON p.Id = t.ProductId
		)

		SELECT * FROM Result
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateWeeklyOpenPurchaseOrdersAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.WeeklyOpenPurchaseOrdersAgg view exists...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.WeeklyOpenPurchaseOrdersAgg AS
        SELECT
            pol.ProductId,
            DATEADD(
                WEEK,
                DATEDIFF(WEEK, 0, po.ExpectedDeliveryDate),
                0
            ) AS WeekStartDate,
            SUM(pol.Quantity - ISNULL(pol.ReceivedQuantity, 0)) AS QuantityOnOrder
        FROM PurchaseOrderLineItem pol
        JOIN PurchaseOrder po ON po.Id = pol.PurchaseOrderId
        WHERE po.Status IN (0,1,2)  
        GROUP BY
            pol.ProductId,
            DATEADD(
                WEEK,
                DATEDIFF(WEEK, 0, po.ExpectedDeliveryDate),
                0
            );
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    #endregion

    #region Procedures

    private static async Task DeleteProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.DeletePurchaseForecasts stored procedure exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.DeletePurchaseForecasts
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            DELETE pf
            FROM PurchaseForecasts pf
            JOIN Products p ON p.Id = pf.ProductId
            WHERE p.CompanyId = @CompanyId
            AND pf.WeekEndDate < CONVERT(date, SYSUTCDATETIME())
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task InsertProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.InsertPurchaseForecasts stored procedure exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.InsertPurchaseForecasts
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            EXEC dbo.DeletePurchaseForecasts @CompanyId, @ProductIds;

            INSERT INTO PurchaseForecasts (
                ProductId,
                WeekStartDate,
                WeekEndDate,
                WeekIndex,
                ForecastQuantity,
                QuantityCommitted,
                QuantityOnOrder,
                ProjectedStock,
                NeedsReorder,
                RecommendedOrderQuantity,
                RecommendedOrderDate,
                CalculatedAt
            )
            SELECT
                wf.ProductId,
                wf.WeekStartDate,
                wf.WeekEndDate,
                wf.WeekIndex,
                wf.ForecastQuantity,
                0,
                0,
                0,          
                0,
                0,
                NULL,
                SYSUTCDATETIME()
            FROM Products p
            INNER JOIN WeeklyForecastAgg wf ON p.Id = wf.ProductId
            WHERE p.CompanyId = @CompanyId
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            )
            AND NOT EXISTS (
                SELECT 1
                FROM PurchaseForecasts pf
                WHERE pf.ProductId = wf.ProductId
                AND pf.WeekStartDate = wf.WeekStartDate
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task UpdateProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.UpdatePurchaseForecasts stored procedure exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[UpdatePurchaseForecasts]
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Prepare Base Data
            SELECT
                wf.ProductId,
                wf.WeekStartDate,
                wf.WeekEndDate,
                wf.WeekIndex,
                wf.WeekIndex + CEILING(pol.LeadTimeDays / 7.0) AS ArrivalWeekIndex,
                wf.ForecastQuantity,

                inv.QuantityOnHand,
                ISNULL(wc.QuantityCommitted, 0) AS QuantityCommitted,
                ISNULL(wo.QuantityOnOrder, 0) AS QuantityOnOrder,

                CASE
                    WHEN ISNULL(wc.QuantityCommitted, 0) > CAST(wf.ForecastQuantity AS FLOAT)
                        THEN ISNULL(wc.QuantityCommitted, 0)
                    ELSE CAST(wf.ForecastQuantity AS FLOAT)
                END AS EffectiveDemand,

                pol.SafetyStock,
                pol.MinimumOrderQuantity AS MOQ,
                pol.OrderIncrementQuantity AS OrderIncrement,
                CEILING(pol.LeadTimeDays / 7.0) AS LeadTimeWeeks
            INTO #Plan
            FROM Products p
            JOIN WeeklyForecastAgg wf ON wf.ProductId = p.Id
            JOIN ProductInventoryPolicies pol ON pol.ProductId = p.Id
            JOIN InventorySnapshot inv ON inv.ProductId = p.Id
            LEFT JOIN WeeklyCommittedAgg wc
                ON wc.ProductId = p.Id
                AND wc.WeekStartDate = wf.WeekStartDate
            LEFT JOIN WeeklyOpenPurchaseOrdersAgg wo
                ON wo.ProductId = p.Id
                AND wo.WeekStartDate = wf.WeekStartDate
            WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )

            -- Add planning columns
            ALTER TABLE #Plan ADD
                ProjectedStock DECIMAL(15,2) NULL,
                NetDemandDuringLeadTime FLOAT NOT NULL DEFAULT 0,
                RecommendedOrderQuantity INT NOT NULL DEFAULT 0,
                ArrivingFromRecommendations INT NOT NULL DEFAULT 0;

            -- Calculate net demand during lead time
            WITH LeadTimeDemand AS (
                SELECT
                    b1.ProductId,
                    b1.WeekIndex,
                    GREATEST(
                        SUM(b2.EffectiveDemand) - 
                        SUM(b2.QuantityOnOrder), 
                        0
                    ) AS NetDemandDuringLeadTime
                FROM #Plan b1
                JOIN #Plan b2
                ON b2.ProductId = b1.ProductId
                AND b2.WeekIndex > b1.WeekIndex
                AND b2.WeekIndex <= b1.WeekIndex + b1.LeadTimeWeeks
                GROUP BY
                    b1.ProductId,
                    b1.WeekIndex
            )
            UPDATE p
            SET p.NetDemandDuringLeadTime = l.NetDemandDuringLeadTime
            FROM LeadTimeDemand l
            INNER JOIN #Plan p	
                ON l.ProductId = p.ProductId
                AND l.WeekIndex = p.WeekIndex

            -- Seed ProjectedStock in week 0
            UPDATE #Plan
            SET ProjectedStock =
                QuantityOnHand
                + QuantityOnOrder
                - EffectiveDemand
            WHERE WeekIndex = 0;


            -- Iterate week by week
            DECLARE @Week INT = 0;
            DECLARE @MaxWeek INT = (SELECT MAX(WeekIndex) FROM #Plan);

            WHILE @Week <= @MaxWeek
            BEGIN

                -- Apply arrivals from previous recommendations
                UPDATE p
                SET p.ArrivingFromRecommendations = ISNULL(a.ArrivingQty, 0)
                FROM #Plan p
                LEFT JOIN (
                    SELECT
                        ProductId,
                        WeekIndex + LeadTimeWeeks AS ArrivalWeekIndex,
                        SUM(RecommendedOrderQuantity) AS ArrivingQty
                    FROM #Plan
                    WHERE RecommendedOrderQuantity > 0
                    GROUP BY
                        ProductId,
                        WeekIndex + LeadTimeWeeks
                ) a
                    ON a.ProductId = p.ProductId
                AND a.ArrivalWeekIndex = p.WeekIndex
                WHERE p.WeekIndex = @Week;

                -- Calculate projected stock
                UPDATE p
                SET p.ProjectedStock =
                    prev.ProjectedStock
                    + p.QuantityOnOrder
                    + p.ArrivingFromRecommendations
                    - p.EffectiveDemand
                FROM #Plan p
                JOIN #Plan prev
					ON prev.WeekIndex = p.WeekIndex - 1
					AND p.ProductId = prev.ProductId
                WHERE p.WeekIndex = @Week;

                -- Decide whether to order
                UPDATE p
                SET RecommendedOrderQuantity =
                    CASE
                        WHEN p.ProjectedStock - ISNULL(p.NetDemandDuringLeadTime, 0) < p.SafetyStock
                        THEN
                            CASE
                                WHEN p.MOQ <= 0 OR p.OrderIncrement <= 0 THEN 0
                                WHEN CEILING(
                                    (p.SafetyStock
                                    - (p.ProjectedStock - p.NetDemandDuringLeadTime))
                                    / p.OrderIncrement
                                ) * p.OrderIncrement < p.MOQ
                                THEN p.MOQ
                                ELSE CEILING(
                                    (p.SafetyStock
                                    - (p.ProjectedStock - p.NetDemandDuringLeadTime))
                                    / p.OrderIncrement
                                ) * p.OrderIncrement
                            END
                        ELSE 0
                    END
                FROM #Plan p
                WHERE p.WeekIndex = @Week;

                SET @Week += 1;
            END

            UPDATE pf
            SET
                pf.ForecastQuantity = p.ForecastQuantity,
                pf.QuantityCommitted = p.QuantityCommitted,
                pf.QuantityOnOrder = p.QuantityOnOrder,
                pf.ProjectedStock = p.ProjectedStock,
                pf.NeedsReorder = CASE WHEN p.RecommendedOrderQuantity > 0 THEN 1 ELSE 0 END,
                pf.RecommendedOrderQuantity = p.RecommendedOrderQuantity,
                pf.RecommendedOrderDate = 
                    CASE 
                        WHEN p.RecommendedOrderQuantity > 0 AND p.WeekStartDate < CONVERT(date, SYSUTCDATETIME())
                            THEN CONVERT(date, SYSUTCDATETIME())
                        WHEN p.RecommendedOrderQuantity > 0 AND p.WeekStartDate >= CONVERT(date, SYSUTCDATETIME())
                            THEN p.WeekStartDate
                    END,
                pf.CalculatedAt = SYSUTCDATETIME()
            FROM PurchaseForecasts pf
            INNER JOIN #Plan p
                ON p.ProductId = pf.ProductId
                AND p.WeekStartDate = pf.WeekStartDate

            DROP TABLE #Plan

        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    #endregion
}
