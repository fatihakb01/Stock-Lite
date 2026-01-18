using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

internal static class ConfirmedOrderImportObjects
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureStagingTableAsync(context, logger);
        await EnsureProcessProcAsync(context, logger);
    }

    private static async Task EnsureStagingTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingConfirmedOrders exists...");

        var sql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingConfirmedOrders')
        BEGIN
            CREATE TABLE dbo.StagingConfirmedOrders
            (
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                OrderNumber NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                OrderDate DATE NOT NULL,
                ExpectedDeliveryDate DATE NULL,
                ReceivedDate DATE NULL,
                Quantity INT NOT NULL,
                SoldQuantity INT NOT NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessConfirmedOrders exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.ProcessConfirmedOrders
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Insert missing orders
            INSERT INTO ConfirmedOrder (
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
                    WHEN SUM((s.Quantity - s.SoldQuantity) % s.Quantity) = 0
                        THEN 0
                    WHEN SUM(s.Quantity - s.SoldQuantity) > 0
                        THEN 1   
                    WHEN SUM(s.Quantity - s.SoldQuantity) = 0
                        THEN 2
                    ELSE 3
                END
            FROM dbo.StagingConfirmedOrders s
            WHERE s.CompanyId = @CompanyId
              AND NOT EXISTS (
                  SELECT 1
                  FROM ConfirmedOrder o
                  WHERE o.CompanyId = s.CompanyId
                    AND o.OrderNumber = s.OrderNumber
              )
            GROUP BY s.CompanyId, s.OrderNumber;

            -- Update existing orders
            UPDATE o
            SET
                o.OrderDate = s.OrderDate,
                o.ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                o.ReceivedDate = s.ReceivedDate,
                o.Status = s.Status
            FROM ConfirmedOrder o
            JOIN (
                SELECT
                    CompanyId,
                    OrderNumber,
                    MIN(OrderDate) AS OrderDate,
                    MIN(ExpectedDeliveryDate) AS ExpectedDeliveryDate,
                    MIN(ReceivedDate) AS ReceivedDate,
                    CASE 
                        WHEN SUM((s.Quantity - s.SoldQuantity) % s.Quantity) = 0
                            THEN 0
                        WHEN SUM(s.Quantity - s.SoldQuantity) > 0
                            THEN 1   
                        WHEN SUM(s.Quantity - s.SoldQuantity) = 0
                            THEN 2
                        ELSE 3
                    END AS Status
                FROM dbo.StagingConfirmedOrders s
                WHERE CompanyId = @CompanyId
                GROUP BY CompanyId, OrderNumber
            ) s ON s.CompanyId = o.CompanyId AND s.OrderNumber = o.OrderNumber;

            -- Insert missing line items
            INSERT INTO ConfirmedOrderLineItem
            (
                ConfirmedOrderId,
                ProductId,
                Quantity,
                SoldQuantity,
                UnitCost
            )
            SELECT
                o.Id,
                p.Id,
                s.Quantity,
                s.SoldQuantity,
                p.Price
            FROM dbo.StagingConfirmedOrders s
            JOIN Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            JOIN ConfirmedOrder o
                ON o.OrderNumber = s.OrderNumber
                AND o.CompanyId = s.CompanyId
            LEFT JOIN ConfirmedOrderLineItem li
                ON li.ConfirmedOrderId = o.Id
                AND li.ProductId = p.Id
            WHERE s.CompanyId = @CompanyId
            AND li.ProductId IS NULL;

            -- Update existing line items
            UPDATE li
            SET
                li.Quantity = s.Quantity,
                li.SoldQuantity = s.SoldQuantity,
                li.UnitCost = p.Price
            FROM ConfirmedOrderLineItem li
            JOIN ConfirmedOrder o
                ON o.Id = li.ConfirmedOrderId
            JOIN Products p
                ON p.Id = li.ProductId
            JOIN dbo.StagingConfirmedOrders s
                ON s.OrderNumber = o.OrderNumber
                AND s.SKU = p.SKU
                AND s.CompanyId = o.CompanyId
            WHERE o.CompanyId = @CompanyId;

            -- Cleanup
            DELETE FROM dbo.StagingConfirmedOrders WHERE CompanyId = @CompanyId;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
