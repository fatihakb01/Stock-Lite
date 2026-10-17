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
                Version INT NOT NULL,
                ImportTimeStamp DATETIME NOT NULL,
                Id UNIQUEIDENTIFIER NOT NULL,
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                OrderNumber NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                OrderDate DATE NOT NULL,
                ExpectedDeliveryDate DATE NULL,
                ReceivedDate DATE NULL,
                Quantity INT NOT NULL,
                SoldQuantity INT NOT NULL,
                UnitCost NUMERIC(18,2) NULL,
                ProductId UNIQUEIDENTIFIER NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessConfirmedOrders exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[ProcessConfirmedOrders]
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Resolve ProductId from SKU
            UPDATE s
            SET s.ProductId = p.Id
            FROM dbo.StagingConfirmedOrders s
            INNER JOIN dbo.Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0;

            -- Update Id in staging table
            -- Prefer existing ConfirmedOrder.Id; otherwise pick MIN of staging Ids deterministically
            WITH OrderIds AS (
                SELECT
                    s.CompanyId,
                    s.OrderNumber,
                    COALESCE(
                        MAX(co.Id),       -- existing order Id wins if present
                        MIN(s.Id)         -- otherwise pick one deterministic staging Id
                    ) AS CanonicalId
                FROM dbo.StagingConfirmedOrders s
                LEFT JOIN dbo.ConfirmedOrder co
                    ON co.CompanyId = s.CompanyId
                    AND co.OrderNumber = s.OrderNumber
                WHERE s.CompanyId = @CompanyId
                    AND s.Version = 0
                GROUP BY s.CompanyId, s.OrderNumber
            )
            UPDATE s
            SET s.Id = c.CanonicalId
            FROM dbo.StagingConfirmedOrders s
            INNER JOIN OrderIds c
                ON c.CompanyId = s.CompanyId
                AND c.OrderNumber = s.OrderNumber
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0;

            -- Build staging temp table
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
                s.SoldQuantity,
                s.UnitCost,
                s.ProductId,
                CASE
                    WHEN SUM(ISNULL(s.SoldQuantity, 0))
                         OVER(PARTITION BY s.Id) = 0 THEN 0
                    WHEN SUM(s.Quantity - ISNULL(s.SoldQuantity, 0))
                         OVER(PARTITION BY s.Id) <= 0 THEN 3
                    WHEN SUM(s.Quantity - ISNULL(s.SoldQuantity, 0))
                         OVER(PARTITION BY s.Id) > 0 THEN 2
                    ELSE 4
                END AS Status
            INTO #StagingConfirmedOrders
            FROM dbo.StagingConfirmedOrders s
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0
                AND s.ProductId IS NOT NULL;

            CREATE CLUSTERED INDEX IX_Id ON #StagingConfirmedOrders(Id);
            CREATE NONCLUSTERED INDEX IX_ProductId ON #StagingConfirmedOrders(ProductId);
            CREATE NONCLUSTERED INDEX IX_OrderNumber ON #StagingConfirmedOrders(OrderNumber);

            -- Insert missing orders
            INSERT INTO dbo.ConfirmedOrder
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
                FROM #StagingConfirmedOrders
            ) s
            WHERE s.rn = 1
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.ConfirmedOrder o WHERE o.Id = s.Id
                );

            -- Update existing orders
            UPDATE o
            SET o.OrderDate = s.OrderDate,
                o.ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                o.ReceivedDate = s.ReceivedDate,
                o.Status = s.Status
            FROM dbo.ConfirmedOrder o
            INNER JOIN (
                SELECT *,
                    ROW_NUMBER() OVER (PARTITION BY Id ORDER BY (SELECT NULL)) AS rn
                FROM #StagingConfirmedOrders
            ) s ON s.Id = o.Id AND s.rn = 1;

            -- Insert missing line items
            INSERT INTO dbo.ConfirmedOrderLineItem
            (
                ConfirmedOrderId, ProductId, Quantity, SoldQuantity, UnitCost
            )
            SELECT
                s.Id, s.ProductId, s.Quantity, s.SoldQuantity, s.UnitCost
            FROM #StagingConfirmedOrders s
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.ConfirmedOrderLineItem li
                WHERE li.ConfirmedOrderId = s.Id
                    AND li.ProductId = s.ProductId
            );

            -- Update existing line items
            UPDATE li
            SET li.Quantity = s.Quantity,
                li.SoldQuantity = s.SoldQuantity,
                li.UnitCost = s.UnitCost
            FROM dbo.ConfirmedOrderLineItem li
            INNER JOIN #StagingConfirmedOrders s
                ON li.ConfirmedOrderId = s.Id
                AND li.ProductId = s.ProductId;

            -- Return affected ProductIds
            SELECT DISTINCT ProductId FROM #StagingConfirmedOrders;

            DROP TABLE #StagingConfirmedOrders;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
