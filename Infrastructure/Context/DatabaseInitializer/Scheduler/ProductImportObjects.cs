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
                Version INT NOT NULL,
                ImportTimeStamp DATETIME NOT NULL,
                ProductId UNIQUEIDENTIFIER NOT NULL,
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                Name NVARCHAR(200) NOT NULL,
                Price DECIMAL(18,2) NOT NULL,
                Description NVARCHAR(MAX) NULL,
                IsActive BIT NOT NULL,
                CreatedAt DATETIME NULL,
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

            -- Update ProductIds in staging table
            UPDATE s
            SET s.ProductId = p.Id
            FROM dbo.StagingProducts s
            JOIN dbo.Products p
                ON s.CompanyId = p.CompanyId
                AND s.SKU = p.SKU
            WHERE p.CompanyId = @CompanyId
            AND s.Version = 0;

            -- Create staging temp table 
            SELECT
                s.Version,
                s.ImportTimeStamp,
                s.ProductId,
                s.CompanyId,
                s.SKU,
                s.Name,
                s.Price,
                s.Description,
                s.IsActive,
                s.CreatedAt,
                s.LeadTimeDays,
                s.MinimumOrderQuantity,
                s.OrderIncrementQuantity,
                s.SafetyStock,
                s.StockAdjustment
            INTO #StagingProducts
            FROM dbo.StagingProducts s
            WHERE s.CompanyId = @CompanyId
            AND s.Version = 0;

            CREATE CLUSTERED INDEX IX_ProductId ON #StagingProducts(ProductId);

            -- Insert missing products
            INSERT INTO Products
            (
                Id,
                CompanyId,
                SKU,
                Name,
                Price,
                Description,
                IsActive,
                CreatedAt
            )
            SELECT
                s.ProductId,
                s.CompanyId,
                s.SKU,
                s.Name,
                s.Price,
                s.Description,
                s.IsActive,
                s.CreatedAt
            FROM #StagingProducts s
            WHERE NOT EXISTS (
                SELECT 1
                FROM Products p
                WHERE p.Id = s.ProductId
            );

            -- Update products
            UPDATE p
            SET
                p.Name = s.Name,
                p.Price = s.Price,
                p.Description = s.Description,
                p.IsActive = s.IsActive,
                p.CreatedAt = s.CreatedAt
            FROM Products p
            JOIN #StagingProducts s ON s.ProductId = p.Id;

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
                s.ProductId,
                s.LeadTimeDays,
                ISNULL(s.MinimumOrderQuantity, 1),
                ISNULL(s.OrderIncrementQuantity, 1),
                s.SafetyStock,
                ISNULL(s.StockAdjustment, 0)
            FROM #StagingProducts s
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.ProductInventoryPolicies ip
                WHERE ip.ProductId = s.ProductId
            );

            -- Update inventory policies
            UPDATE ip
            SET
                ip.LeadTimeDays = s.LeadTimeDays,
                ip.MinimumOrderQuantity = ISNULL(s.MinimumOrderQuantity, 1),
                ip.OrderIncrementQuantity = ISNULL(s.OrderIncrementQuantity, 1),
                ip.SafetyStock = s.SafetyStock,
                ip.StockAdjustment = ISNULL(s.StockAdjustment, 0)
            FROM ProductInventoryPolicies ip
            JOIN #StagingProducts s ON s.ProductId = ip.ProductId;

            SELECT ProductId FROM #StagingProducts;

            DROP TABLE #StagingProducts;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
