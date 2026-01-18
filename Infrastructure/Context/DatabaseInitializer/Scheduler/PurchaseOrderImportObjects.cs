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
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                OrderNumber NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                OrderDate DATE NOT NULL,
                ExpectedDeliveryDate DATE NOT NULL,
                ReceivedDate DATE NULL,
                Quantity INT NOT NULL,
                ReceivedQuantity INT NOT NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessPurchaseOrders exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.ProcessPurchaseOrders
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Insert missing orders
            INSERT INTO PurchaseOrder
            (
                Id,
                CompanyId,
                OrderNumber,
                OrderDate,
                ExpectedDeliveryDate,
                ReceivedDate,
                Status
            )
            SELECT
                NEWID(),
                s.CompanyId,
                s.OrderNumber,
                MIN(s.OrderDate),
                MIN(s.ExpectedDeliveryDate),
                MIN(s.ReceivedDate),
                CASE 
                    WHEN SUM((s.Quantity - s.ReceivedQuantity) % s.Quantity) = 0
                        THEN 0
                    WHEN SUM(s.Quantity - s.ReceivedQuantity) > 0
                        THEN 2
                    WHEN SUM(s.Quantity - s.ReceivedQuantity) = 0
                        THEN 3
                    ELSE 4
                END AS Status
            FROM dbo.StagingPurchaseOrders s
            WHERE s.CompanyId = @CompanyId
            AND NOT EXISTS (
                SELECT 1
                FROM PurchaseOrder o
                WHERE o.CompanyId = s.CompanyId
                    AND o.OrderNumber = s.OrderNumber
            )
            GROUP BY s.CompanyId, s.OrderNumber;

            -- Update orders
            UPDATE o
            SET
                o.OrderDate = s.OrderDate,
                o.ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                o.ReceivedDate = s.ReceivedDate,
                o.Status = s.Status
            FROM PurchaseOrder o
            JOIN (
                SELECT
                    CompanyId,
                    OrderNumber,
                    MIN(OrderDate) AS OrderDate,
                    MIN(ExpectedDeliveryDate) AS ExpectedDeliveryDate,
                    MIN(ReceivedDate) AS ReceivedDate,
                    CASE 
                        WHEN SUM((s.Quantity - s.ReceivedQuantity) % s.Quantity) = 0
                            THEN 0
                        WHEN SUM(s.Quantity - s.ReceivedQuantity) > 0
                            THEN 2
                        WHEN SUM(s.Quantity - s.ReceivedQuantity) = 0
                            THEN 3
                        ELSE 4
                    END AS Status
                FROM dbo.StagingPurchaseOrders s
                WHERE CompanyId = @CompanyId
                GROUP BY CompanyId, OrderNumber
            ) s ON s.CompanyId = o.CompanyId
            AND s.OrderNumber = o.OrderNumber;

            -- Insert missing line items
            INSERT INTO PurchaseOrderLineItem
            (
                PurchaseOrderId,
                ProductId,
                Quantity,
                ReceivedQuantity,
                UnitCost
            )
            SELECT
                o.Id,
                p.Id,
                s.Quantity,
                s.ReceivedQuantity,
                p.Price
            FROM dbo.StagingPurchaseOrders s
            JOIN Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            JOIN PurchaseOrder o
                ON o.OrderNumber = s.OrderNumber
                AND o.CompanyId = s.CompanyId
            LEFT JOIN PurchaseOrderLineItem li
                ON li.PurchaseOrderId = o.Id
                AND li.ProductId = p.Id
            WHERE s.CompanyId = @CompanyId
            AND li.ProductId IS NULL;

            -- Update line items
            UPDATE li
            SET
                li.Quantity = s.Quantity,
                li.ReceivedQuantity = s.ReceivedQuantity,
                li.UnitCost = p.Price
            FROM PurchaseOrderLineItem li
            JOIN PurchaseOrder o ON o.Id = li.PurchaseOrderId
            JOIN Products p ON p.Id = li.ProductId
            JOIN dbo.StagingPurchaseOrders s
                ON s.OrderNumber = o.OrderNumber
                AND s.SKU = p.SKU
                AND s.CompanyId = o.CompanyId
            WHERE o.CompanyId = @CompanyId;

            DELETE FROM dbo.StagingPurchaseOrders WHERE CompanyId = @CompanyId;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
