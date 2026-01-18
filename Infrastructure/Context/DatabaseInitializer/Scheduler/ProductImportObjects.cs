using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

public class ProductImportObjects
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureStagingTableAsync(context, logger);
        await EnsureProcessProcAsync(context, logger);
    }

    private static async Task EnsureStagingTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingProducts exists...");

        var sql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingProducts')
        BEGIN
            CREATE TABLE dbo.StagingProducts
            (
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                Name NVARCHAR(200) NOT NULL,
                Price DECIMAL(18,2) NOT NULL,
                Description NVARCHAR(MAX) NULL,
                IsActive BIT NOT NULL,
                LeadTimeDays INT NULL,
                MinimumOrderQuantity INT NULL,
                OrderIncrementQuantity INT NULL,
                SafetyStock INT NULL,
                StockAdjustment INT NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessProducts exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.ProcessProducts
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Insert missing products
            INSERT INTO Products
            (
                Id,
                CompanyId,
                SKU,
                Name,
                Price,
                Description,
                IsActive
            )
            SELECT
                NEWID(),
                s.CompanyId,
                s.SKU,
                s.Name,
                s.Price,
                ISNULL(s.Description, ''),
                ISNULL(s.IsActive, 1)
            FROM dbo.StagingProducts s
            WHERE s.CompanyId = @CompanyId
            AND NOT EXISTS (
                SELECT 1
                FROM Products p
                WHERE p.CompanyId = s.CompanyId
                    AND p.SKU = s.SKU
            );

            -- Update products
            UPDATE p
            SET
                p.Name = s.Name,
                p.Price = s.Price,
                p.Description = ISNULL(s.Description, ''),
                p.IsActive = ISNULL(s.IsActive, 1)
            FROM Products p
            JOIN dbo.StagingProducts s
                ON s.CompanyId = p.CompanyId
                AND s.SKU = p.SKU
            WHERE p.CompanyId = @CompanyId;

            -- Insert missing inventory policies
            INSERT INTO ProductInventoryPolicies
            (
                ProductId,
                LeadTimeDays,
                MinimumOrderQuantity,
                OrderIncrementQuantity,
                SafetyStock,
                StockAdjustment
            )
            SELECT
                p.Id,
                s.LeadTimeDays,
                ISNULL(s.MinimumOrderQuantity, 1),
                ISNULL(s.OrderIncrementQuantity, 1),
                s.SafetyStock,
                ISNULL(s.StockAdjustment, 0)
            FROM dbo.StagingProducts s
            JOIN Products p
                ON p.CompanyId = s.CompanyId
                AND p.SKU = s.SKU
            LEFT JOIN ProductInventoryPolicies ip
                ON ip.ProductId = p.Id
            WHERE s.CompanyId = @CompanyId
            AND ip.ProductId IS NULL;

            -- Update inventory policies
            UPDATE ip
            SET
                ip.LeadTimeDays = s.LeadTimeDays,
                ip.MinimumOrderQuantity = ISNULL(s.MinimumOrderQuantity, 1),
                ip.OrderIncrementQuantity = ISNULL(s.OrderIncrementQuantity, 1),
                ip.SafetyStock = s.SafetyStock,
                ip.StockAdjustment = ISNULL(s.StockAdjustment, 0)
            FROM ProductInventoryPolicies ip
            JOIN Products p ON p.Id = ip.ProductId
            JOIN dbo.StagingProducts s
                ON s.CompanyId = p.CompanyId
                AND s.SKU = p.SKU
            WHERE p.CompanyId = @CompanyId;

            DELETE FROM dbo.StagingProducts WHERE CompanyId = @CompanyId;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
