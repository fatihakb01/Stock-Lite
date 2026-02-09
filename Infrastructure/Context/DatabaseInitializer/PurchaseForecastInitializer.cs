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
        CREATE OR ALTER PROCEDURE dbo.UpdatePurchaseForecasts
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            WITH BaseData AS (
                SELECT
                    wf.ProductId,
                    wf.WeekStartDate,
                    wf.WeekEndDate,
                    wf.WeekIndex,
                    wf.ForecastQuantity,
					inv.QuantityOnHand,
                    ISNULL(wc.QuantityCommitted, 0) AS QuantityCommitted,
					ISNULL(wo.QuantityOnOrder, 0) AS QuantityOnOrder,
					CASE
						WHEN ISNULL(wc.QuantityCommitted, 0) > wf.ForecastQuantity
							THEN ISNULL(wc.QuantityCommitted, 0)
						ELSE CAST(wf.ForecastQuantity AS FLOAT)
					END AS EffectiveDemand,
                    pol.SafetyStock,
                    pol.MinimumOrderQuantity AS MOQ,
                    pol.OrderIncrementQuantity AS OrderIncrement
                FROM Products p
                JOIN WeeklyForecastAgg wf ON wf.ProductId = p.Id
                JOIN ProductInventoryPolicies pol ON pol.ProductId = p.Id
				JOIN InventorySnapshot inv ON inv.ProductId = p.Id
				INNER JOIN PurchaseForecasts pf
					ON pf.ProductId = p.Id
					AND pf.WeekStartDate = wf.WeekStartDate
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
            ), Seed AS (
                SELECT
                    b.ProductId,
                    b.WeekStartDate,
                    b.WeekEndDate,
                    b.WeekIndex,
                    b.ForecastQuantity,
                    b.QuantityCommitted,
                    b.QuantityOnOrder,
                    b.SafetyStock,
                    b.MOQ,
                    b.OrderIncrement,

                    -- Initial projected stock
					b.QuantityOnHand
					+ ISNULL(b.QuantityOnOrder, 0)
					- b.EffectiveDemand
					AS ProjectedStock,

                    CAST(
                        CASE
                            WHEN b.QuantityOnHand + ISNULL(b.QuantityOnOrder, 0) - b.ForecastQuantity < b.SafetyStock
                            THEN
                                CASE
                                    WHEN b.MOQ <= 0 OR b.OrderIncrement <= 0 THEN 0
                                    WHEN CEILING((b.SafetyStock - (b.QuantityOnHand + ISNULL(b.QuantityOnOrder, 0) - b.ForecastQuantity))
                                        / b.OrderIncrement) * b.OrderIncrement < b.MOQ
                                    THEN b.MOQ
                                    ELSE CEILING((b.SafetyStock - (b.QuantityOnHand + ISNULL(b.QuantityOnOrder, 0) - b.ForecastQuantity))
                                        / b.OrderIncrement) * b.OrderIncrement
                                END
                            ELSE 0
                        END AS INT
                    ) AS RecommendedOrderQuantity,

                    CAST(
                        CASE
                            WHEN b.QuantityOnHand + ISNULL(b.QuantityOnOrder, 0) - b.ForecastQuantity < b.SafetyStock
                            THEN 1 ELSE 0
                        END AS BIT
                    ) AS NeedsReorder
                FROM BaseData b
                WHERE b.WeekIndex = 0
            ), RecursivePlan AS (
                -- Anchor
                SELECT * FROM Seed

                UNION ALL

                -- Recursive step
                SELECT
                    b.ProductId,
                    b.WeekStartDate,
                    b.WeekEndDate,
                    b.WeekIndex,
                    b.ForecastQuantity,
                    b.QuantityCommitted,
                    rp.QuantityOnOrder,
                    b.SafetyStock,
                    b.MOQ,
                    b.OrderIncrement,

                    -- Projected stock rolls forward
					rp.ProjectedStock
					+ rp.RecommendedOrderQuantity
					+ ISNULL(b.QuantityOnOrder, 0)
					- b.EffectiveDemand
					AS ProjectedStock,


                    -- Recommended order
                    CAST(
                        CASE
                            WHEN rp.ProjectedStock + rp.RecommendedOrderQuantity - b.ForecastQuantity < b.SafetyStock
                            THEN
                                CASE
                                    WHEN b.MOQ <= 0 OR b.OrderIncrement <= 0 THEN 0
                                    WHEN CEILING(
                                        (b.SafetyStock - (rp.ProjectedStock + rp.RecommendedOrderQuantity - b.ForecastQuantity))
                                        / b.OrderIncrement
                                    ) * b.OrderIncrement < b.MOQ
                                    THEN b.MOQ
                                    ELSE CEILING(
                                        (b.SafetyStock - (rp.ProjectedStock + rp.RecommendedOrderQuantity - b.ForecastQuantity))
                                        / b.OrderIncrement
                                    ) * b.OrderIncrement
                                END
                            ELSE 0
                        END AS INT
                    ) AS RecommendedOrderQuantity,

                    CAST(
                        CASE
                            WHEN rp.ProjectedStock + rp.RecommendedOrderQuantity - b.ForecastQuantity < b.SafetyStock
                            THEN 1 ELSE 0
                        END AS BIT
                    ) AS NeedsReorder
                FROM RecursivePlan rp
                JOIN BaseData b
                ON b.ProductId = rp.ProductId
                AND b.WeekIndex = rp.WeekIndex + 1
            )

            UPDATE pf
            SET
                pf.ForecastQuantity = r.ForecastQuantity,
                pf.QuantityCommitted = r.QuantityCommitted,
                pf.QuantityOnOrder = r.QuantityOnOrder,
                pf.ProjectedStock = r.ProjectedStock,
                pf.NeedsReorder = r.NeedsReorder,
                pf.RecommendedOrderQuantity = r.RecommendedOrderQuantity,
                pf.RecommendedOrderDate = 
                    CASE 
                        WHEN r.NeedsReorder = 1 AND r.WeekStartDate < CONVERT(date, SYSUTCDATETIME())
                            THEN CONVERT(date, SYSUTCDATETIME())
                        WHEN r.NeedsReorder = 1 AND r.WeekStartDate >= CONVERT(date, SYSUTCDATETIME())
                            THEN r.WeekStartDate
                    END,
                pf.CalculatedAt = SYSUTCDATETIME()
            FROM PurchaseForecasts pf
            INNER JOIN RecursivePlan r
                ON r.ProductId = pf.ProductId
                AND r.WeekStartDate = pf.WeekStartDate
            OPTION (MAXRECURSION 1000);
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    #endregion
}
