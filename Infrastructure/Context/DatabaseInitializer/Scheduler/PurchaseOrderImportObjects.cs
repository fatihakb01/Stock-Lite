using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

public class PurchaseOrderImportObjects
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureStagingTableAsync(context, logger);
        await EnsureProcessProcAsync(context, logger);
    }

    private static async Task EnsureStagingTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingPurchaseOrders exists...");

        var sql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingPurchaseOrders')
        BEGIN
            CREATE TABLE dbo.StagingPurchaseOrders
            (
                Version INT NOT NULL,
                ImportTimeStamp DATETIME NOT NULL,
                Id UNIQUEIDENTIFIER NOT NULL,
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                OrderNumber NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                OrderDate DATE NOT NULL,
                ExpectedDeliveryDate DATE NOT NULL,
                ReceivedDate DATE NULL,
                Quantity INT NOT NULL,
                ReceivedQuantity INT NOT NULL,
				UnitCost NUMERIC(18,2) NULL,
				ProductId UNIQUEIDENTIFIER NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessPurchaseOrders exists...");

        var sql = @"
		CREATE OR ALTER PROCEDURE [dbo].[ProcessPurchaseOrders]
			@CompanyId UNIQUEIDENTIFIER
		AS
		BEGIN
			SET NOCOUNT ON;

			-- Update ProductId in staging table
			UPDATE s
			SET s.ProductId = p.Id
			FROM dbo.StagingPurchaseOrders s
			INNER JOIN dbo.Products p
				ON p.SKU = s.SKU
				AND p.CompanyId = s.CompanyId
			WHERE s.CompanyId = @CompanyId
				AND s.Version = 0;

			-- Update Id in staging table
			-- Prefer existing PurchaseOrder.Id; otherwise pick MIN of staging Ids deterministically
			WITH Orders AS (
				SELECT
					s.CompanyId,
					s.OrderNumber,
					COALESCE(
						MAX(po.Id),
						MIN(s.Id) 
					) AS CanonicalId
				FROM dbo.StagingPurchaseOrders s
				LEFT JOIN dbo.PurchaseOrder po
					ON po.CompanyId = s.CompanyId
					AND po.OrderNumber = s.OrderNumber
				WHERE s.CompanyId = @CompanyId
					AND s.Version = 0
				GROUP BY s.CompanyId, s.OrderNumber
			)
			UPDATE s
			SET s.Id = c.CanonicalId
			FROM dbo.StagingPurchaseOrders s
			INNER JOIN Orders c
				ON c.CompanyId = s.CompanyId
				AND c.OrderNumber = s.OrderNumber
			WHERE s.CompanyId = @CompanyId
				AND s.Version = 0;

			-- Create staging temp table with all necessary data
			SELECT
				s.Version,
				s.ImportTimeStamp,
				s.Id,
				s.CompanyId,
				s.OrderNumber,
				s.SKU,
				s.OrderDate,
				s.ExpectedDeliveryDate,
				s.ReceivedDate,
				s.Quantity,
				s.ReceivedQuantity,
				s.UnitCost,
				s.ProductId,
				CASE
					WHEN SUM(s.ReceivedQuantity) OVER(PARTITION BY s.Id) = 0 THEN 0
					WHEN SUM(s.Quantity - s.ReceivedQuantity) OVER(PARTITION BY s.Id) <= 0 THEN 3
					WHEN SUM(s.Quantity - s.ReceivedQuantity) OVER(PARTITION BY s.Id) > 0 THEN 2
					ELSE 4
				END AS Status
			INTO #StagingPurchaseOrders
			FROM dbo.StagingPurchaseOrders s
			WHERE s.CompanyId = @CompanyId
				AND s.Version = 0
				AND s.ProductId IS NOT NULL;

			-- Add indexes for performance
			CREATE CLUSTERED INDEX IX_Id ON #StagingPurchaseOrders(Id);
			CREATE NONCLUSTERED INDEX IX_ProductId ON #StagingPurchaseOrders(ProductId);
			CREATE NONCLUSTERED INDEX IX_OrderNumber ON #StagingPurchaseOrders(OrderNumber);

			-- Insert missing orders
			INSERT INTO dbo.PurchaseOrder
			(
				Id, CompanyId, OrderNumber, OrderDate,
				ExpectedDeliveryDate, ReceivedDate, [Status]
			)
			SELECT
				s.Id,
				s.CompanyId,
				s.OrderNumber,
				s.OrderDate,
				s.ExpectedDeliveryDate,
				s.ReceivedDate,
				s.[Status]
			FROM (
				SELECT *,
					ROW_NUMBER() OVER (PARTITION BY Id ORDER BY (SELECT NULL)) AS rn
				FROM #StagingPurchaseOrders
			) s
			WHERE s.rn = 1
				AND NOT EXISTS (
					SELECT 1 FROM dbo.PurchaseOrder o WHERE o.Id = s.Id
				);

			-- Update existing orders
			UPDATE o
			SET o.OrderDate = s.OrderDate,
				o.ExpectedDeliveryDate = s.ExpectedDeliveryDate,
				o.ReceivedDate = s.ReceivedDate,
				o.Status = s.Status
			FROM dbo.PurchaseOrder o
			INNER JOIN (
				SELECT *,
					ROW_NUMBER() OVER (PARTITION BY Id ORDER BY (SELECT NULL)) AS rn
				FROM #StagingPurchaseOrders
			) s ON s.Id = o.Id AND s.rn = 1;

			-- Insert missing line items
			INSERT INTO dbo.PurchaseOrderLineItem
			(
				PurchaseOrderId, ProductId, Quantity, ReceivedQuantity, UnitCost
			)
			SELECT
				s.Id,
				s.ProductId,
				s.Quantity,
				s.ReceivedQuantity,
				s.UnitCost
			FROM #StagingPurchaseOrders s
			WHERE NOT EXISTS (
				SELECT 1
				FROM dbo.PurchaseOrderLineItem li
				WHERE li.PurchaseOrderId = s.Id
					AND li.ProductId = s.ProductId
			);

			-- Update existing line items
			UPDATE li
			SET li.Quantity = s.Quantity,
				li.ReceivedQuantity = s.ReceivedQuantity,
				li.UnitCost = s.UnitCost
			FROM dbo.PurchaseOrderLineItem li
			INNER JOIN #StagingPurchaseOrders s
				ON li.PurchaseOrderId = s.Id
				AND li.ProductId = s.ProductId;

			-- Return affected ProductIds
			SELECT DISTINCT ProductId FROM #StagingPurchaseOrders;

			DROP TABLE #StagingPurchaseOrders;
		END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
