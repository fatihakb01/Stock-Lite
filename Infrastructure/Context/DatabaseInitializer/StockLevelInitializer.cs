using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class StockLevelInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await CreateInventorySnapshot(context, logger);
        await CreateOpenPurchaseOrdersAgg(context, logger);
        await CreateConfirmedOrdersAgg(context, logger);
        await CreateForecastDemandLeadTimeAgg(context, logger);

        await DeleteProcAsync(context, logger);
        await InsertProcAsync(context, logger);
        await UpdateProcAsync(context, logger);
    }

    private static async Task CreateInventorySnapshot(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.InventorySnapshot view exist...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.InventorySnapshot AS
        SELECT
            p.Id AS ProductId,

            -- Received from purchase orders
            ISNULL(SUM(pol.ReceivedQuantity), 0)

            -- Minus sold via transactions
            - ISNULL(SUM(tli.Quantity), 0)

            AS QuantityOnHand
        FROM Products p
        LEFT JOIN PurchaseOrderLineItem pol
            ON pol.ProductId = p.Id
        LEFT JOIN PurchaseOrder po
            ON po.Id = pol.PurchaseOrderId
            AND po.Status = 3
        LEFT JOIN TransactionLineItems tli
            ON tli.ProductId = p.Id
        GROUP BY p.Id;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateOpenPurchaseOrdersAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.OpenPurchaseOrdersAgg view exist...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.OpenPurchaseOrdersAgg AS
        SELECT
            pol.ProductId,
            SUM(pol.Quantity - ISNULL(pol.ReceivedQuantity, 0)) AS QuantityOnOrder
        FROM PurchaseOrderLineItem pol
        JOIN PurchaseOrder po ON po.Id = pol.PurchaseOrderId
        WHERE po.Status IN (0, 1, 2) 
        GROUP BY pol.ProductId;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateConfirmedOrdersAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ConfirmedOrdersAgg view exist...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.ConfirmedOrdersAgg AS
        SELECT
            col.ProductId,
            SUM(col.Quantity - ISNULL(col.SoldQuantity, 0)) AS QuantityCommitted
        FROM ConfirmedOrderLineItem col
        JOIN ConfirmedOrder co ON co.Id = col.ConfirmedOrderId
        WHERE co.Status IN (0, 1)
        GROUP BY col.ProductId;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CreateForecastDemandLeadTimeAgg(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ForecastDemandLeadTimeAgg view exist...");

        var sql = @"
        CREATE OR ALTER VIEW dbo.ForecastDemandLeadTimeAgg AS
        WITH Policy AS (
            SELECT ProductId, LeadTimeDays
            FROM ProductInventoryPolicies
        ),
        DailyDemand AS (
            SELECT
                f.ProductId,
                SUM(f.PredictedQuantity) AS Qty
            FROM Forecasts f
            JOIN Policy p ON p.ProductId = f.ProductId
            WHERE f.PeriodType = 'Daily'
            AND f.PeriodStartDate >= CONVERT(date, SYSUTCDATETIME())
            AND f.PeriodStartDate < DATEADD(DAY, p.LeadTimeDays, CONVERT(date, SYSUTCDATETIME()))
            AND p.LeadTimeDays <= 30
            GROUP BY f.ProductId
        ),
        WeeklyDemand AS (
            SELECT
                f.ProductId,
                SUM(f.PredictedQuantity) AS Qty
            FROM Forecasts f
            JOIN Policy p ON p.ProductId = f.ProductId
            WHERE f.PeriodType = 'Weekly'
            AND (p.LeadTimeDays BETWEEN 31 AND 91)
            AND f.PeriodStartDate >= DATEADD(DAY, 30, CONVERT(date, SYSUTCDATETIME()))
            AND f.PeriodStartDate < DATEADD(DAY, p.LeadTimeDays, CONVERT(date, SYSUTCDATETIME()))
            GROUP BY f.ProductId
        ),
        MonthlyDemand AS (
            SELECT
                f.ProductId,
                SUM(f.PredictedQuantity) AS Qty
            FROM Forecasts f
            JOIN Policy p ON p.ProductId = f.ProductId
            WHERE f.PeriodType = 'Monthly'
            AND (p.LeadTimeDays BETWEEN 92 AND 365)
            AND f.PeriodStartDate >= DATEADD(DAY, 91, CONVERT(date, SYSUTCDATETIME()))
            AND f.PeriodStartDate < DATEADD(DAY, p.LeadTimeDays, CONVERT(date, SYSUTCDATETIME()))
            GROUP BY f.ProductId
        )
        SELECT
            ProductId,
            SUM(Qty) AS ForecastQty
        FROM (
            SELECT * FROM DailyDemand
            UNION ALL
            SELECT * FROM WeeklyDemand
            UNION ALL
            SELECT * FROM MonthlyDemand
        ) d
        GROUP BY ProductId;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task DeleteProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.DeleteProcAsync stored procedures exist...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.DeleteStockLevels
            @CompanyId UNIQUEIDENTIFIER,
            @ProductIds dbo.GuidList READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            DELETE sl
            FROM StockLevel sl
            JOIN Products p ON p.Id = sl.ProductId
            WHERE p.CompanyId = @CompanyId
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            )
        END
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
                ProjectedStockAtLeadTime,
                ReorderPoint,
                NeedsReorder,
                RecommendedOrderQuantity,
                LastCalculatedAt
            )
            SELECT
                p.Id AS ProductId,
				ISNULL(inv.QuantityOnHand, 0),
				ISNULL(po.QuantityOnOrder, 0),
				ISNULL(co.QuantityCommitted, 0),

                -- NetStockPosition
                ISNULL(inv.QuantityOnHand, 0)
                + ISNULL(po.QuantityOnOrder, 0)
                - ISNULL(co.QuantityCommitted, 0)
                + pol.StockAdjustment,

                -- ProjectedStockAtLeadTime
                (
                    ISNULL(inv.QuantityOnHand, 0)
                    + ISNULL(po.QuantityOnOrder, 0)
                    - ISNULL(co.QuantityCommitted, 0)
                    + pol.StockAdjustment
                    - ISNULL(fd.ForecastQty, 0)
                ),

                -- ReorderPoint
                ISNULL(fd.ForecastQty, 0) + pol.SafetyStock,

                -- NeedsReorder
                CASE
                    WHEN (
                        ISNULL(inv.QuantityOnHand, 0)
                        + ISNULL(po.QuantityOnOrder, 0)
                        - ISNULL(co.QuantityCommitted, 0)
                        + pol.StockAdjustment
                        - ISNULL(fd.ForecastQty, 0)
                    ) < pol.SafetyStock
                    THEN 1 ELSE 0
                END,

				-- RecommendedOrderQuantity
				CASE
					WHEN pol.OrderIncrementQuantity <= 0
						 OR pol.MinimumOrderQuantity <= 0
						 OR   
                            -- NeedsReorder                 
							CASE
								WHEN (
									ISNULL(inv.QuantityOnHand, 0)
									+ ISNULL(po.QuantityOnOrder, 0)
									- ISNULL(co.QuantityCommitted, 0)
									+ pol.StockAdjustment
									- ISNULL(fd.ForecastQty, 0)
								) < pol.SafetyStock
								THEN 1 ELSE 0 END = 0							
					THEN 0
					ELSE
						CASE
							WHEN
								CEILING(
									CAST(
										(ISNULL(fd.ForecastQty, 0) + pol.SafetyStock)
										AS FLOAT
									) / pol.OrderIncrementQuantity
								) * pol.OrderIncrementQuantity
								< pol.MinimumOrderQuantity
							THEN pol.MinimumOrderQuantity
							ELSE
								CEILING(
									CAST(
										(ISNULL(fd.ForecastQty, 0) + pol.SafetyStock)
										AS FLOAT
									) / pol.OrderIncrementQuantity
								) * pol.OrderIncrementQuantity
						END
				END,
                SYSUTCDATETIME()
            FROM Products p
            INNER JOIN ProductInventoryPolicies pol ON pol.ProductId = p.Id
			LEFT JOIN StockLevels s ON s.ProductId = p.Id
            LEFT JOIN InventorySnapshot inv ON inv.ProductId = p.Id
            LEFT JOIN OpenPurchaseOrdersAgg po ON po.ProductId = p.Id
            LEFT JOIN ConfirmedOrdersAgg co ON co.ProductId = p.Id
            LEFT JOIN ForecastDemandLeadTimeAgg fd ON fd.ProductId = p.Id
			WHERE s.ProductId IS NULL
            AND p.CompanyId = @CompanyId
            AND (
                NOT EXISTS (SELECT 1 FROM @ProductIds)
                OR p.Id IN (SELECT Id FROM @ProductIds)
            )
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
				sl.QuantityOnHand = calc.QuantityOnHand,
				sl.QuantityOnOrder = calc.QuantityOnOrder,
				sl.QuantityCommitted = calc.QuantityCommitted,
                sl.NetStockPosition = calc.NetStockPosition,
                sl.ProjectedStockAtLeadTime = calc.ProjectedStockAtLeadTime,
                sl.ReorderPoint = calc.ReorderPoint,
                sl.NeedsReorder = calc.NeedsReorder,
                sl.RecommendedOrderQuantity = calc.RecommendedOrderQuantity,
                sl.LastCalculatedAt = SYSUTCDATETIME()
            FROM StockLevels sl
            JOIN (
                SELECT
                    p.Id AS ProductId,
					ISNULL(inv.QuantityOnHand, 0) AS QuantityOnHand,
					ISNULL(po.QuantityOnOrder, 0) AS QuantityOnOrder,
					ISNULL(co.QuantityCommitted, 0) AS QuantityCommitted,

                    -- NetStockPosition
                    ISNULL(inv.QuantityOnHand, 0)
                    + ISNULL(po.QuantityOnOrder, 0)
                    - ISNULL(co.QuantityCommitted, 0)
                    + pol.StockAdjustment AS NetStockPosition,

                    -- ProjectedStockAtLeadTime
                    (
                        ISNULL(inv.QuantityOnHand, 0)
                        + ISNULL(po.QuantityOnOrder, 0)
                        - ISNULL(co.QuantityCommitted, 0)
                        + pol.StockAdjustment
                        - ISNULL(fd.ForecastQty, 0)
                    ) AS ProjectedStockAtLeadTime,

                    -- ReorderPoint
                    ISNULL(fd.ForecastQty, 0) + pol.SafetyStock AS ReorderPoint,

                    -- NeedsReorder
                    CASE
                        WHEN (
                            ISNULL(inv.QuantityOnHand, 0)
                            + ISNULL(po.QuantityOnOrder, 0)
                            - ISNULL(co.QuantityCommitted, 0)
                            + pol.StockAdjustment
                            - ISNULL(fd.ForecastQty, 0)
                        ) < pol.SafetyStock
                        THEN 1 ELSE 0
                    END AS NeedsReorder,

					-- RecommendedOrderQuantity
					CASE
						WHEN pol.OrderIncrementQuantity <= 0
							 OR pol.MinimumOrderQuantity <= 0
							 OR        
                                -- NeedsReorder             
								CASE
									WHEN (
										ISNULL(inv.QuantityOnHand, 0)
										+ ISNULL(po.QuantityOnOrder, 0)
										- ISNULL(co.QuantityCommitted, 0)
										+ pol.StockAdjustment
										- ISNULL(fd.ForecastQty, 0)
									) < pol.SafetyStock
								THEN 1 ELSE 0 END = 0							
						THEN 0
						ELSE
							CASE
								WHEN
									CEILING(
										CAST(
											(ISNULL(fd.ForecastQty, 0) + pol.SafetyStock)
											AS FLOAT
										) / pol.OrderIncrementQuantity
									) * pol.OrderIncrementQuantity
									< pol.MinimumOrderQuantity
								THEN pol.MinimumOrderQuantity
								ELSE
									CEILING(
										CAST(
											(ISNULL(fd.ForecastQty, 0) + pol.SafetyStock)
											AS FLOAT
										) / pol.OrderIncrementQuantity
									) * pol.OrderIncrementQuantity
							END
					END AS RecommendedOrderQuantity
                FROM Products p
                INNER JOIN ProductInventoryPolicies pol ON pol.ProductId = p.Id
				INNER JOIN StockLevels s ON s.ProductId = p.Id
                LEFT JOIN InventorySnapshot inv ON inv.ProductId = p.Id
                LEFT JOIN OpenPurchaseOrdersAgg po ON po.ProductId = p.Id
                LEFT JOIN ConfirmedOrdersAgg co ON co.ProductId = p.Id
                LEFT JOIN ForecastDemandLeadTimeAgg fd ON fd.ProductId = p.Id
                WHERE p.CompanyId = @CompanyId
                AND (
                    NOT EXISTS (SELECT 1 FROM @ProductIds)
                    OR p.Id IN (SELECT Id FROM @ProductIds)
                )
            ) calc ON calc.ProductId = sl.ProductId
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
