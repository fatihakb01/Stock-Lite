using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

public class TransactionImportObjects
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureStagingTableAsync(context, logger);
        await EnsureProcessProcAsync(context, logger);
    }

    private static async Task EnsureStagingTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingTransactions exists...");

        var sql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingTransactions')
        BEGIN
            CREATE TABLE dbo.StagingTransactions
            (
                Version INT NOT NULL,
                ImportTimeStamp DATETIME NOT NULL,
                Id UNIQUEIDENTIFIER NOT NULL,
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                TransactionId NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                TransactionDate DATE NOT NULL,
                PaymentMethod NVARCHAR(50) NULL,
                Notes NVARCHAR(200) NULL,
				Quantity INT NOT NULL,
				Price NUMERIC(18,2) NULL,
				ProductId UNIQUEIDENTIFIER NULL,
				TotalAmount DECIMAL(18,2) NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessTransactions exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[ProcessTransactions]
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Resolve ProductId from SKU
            UPDATE s
            SET s.ProductId = p.Id
            FROM dbo.StagingTransactions s
            INNER JOIN dbo.Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0;

            -- Update Id in staging table
            -- Prefer existing Transactions.Id; otherwise pick MIN of staging Ids deterministically
            WITH TransactionIds AS (
                SELECT
                    s.CompanyId,
                    s.TransactionId,
                    COALESCE(
                        MAX(t.Id),        -- existing transaction Id wins if present
                        MIN(s.Id)         -- otherwise pick one deterministic staging Id
                    ) AS CanonicalId
                FROM dbo.StagingTransactions s
                LEFT JOIN dbo.Transactions t
                    ON t.CompanyId = s.CompanyId
                    AND t.TransactionId = s.TransactionId
                WHERE s.CompanyId = @CompanyId
                    AND s.Version = 0
                GROUP BY s.CompanyId, s.TransactionId
            )
            UPDATE s
            SET s.Id = c.CanonicalId
            FROM dbo.StagingTransactions s
            INNER JOIN TransactionIds c
                ON c.CompanyId = s.CompanyId
                AND c.TransactionId = s.TransactionId
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0;

            -- Build staging temp table
            SELECT
                s.Version,
                s.ImportTimeStamp,
                s.Id,
                s.CompanyId,
                s.TransactionId,
                s.SKU,
                s.TransactionDate,
                s.PaymentMethod,
                s.Notes,
                s.Quantity,
                s.ProductId,
                p.Price,
                ISNULL(p.Price * s.Quantity, 0) AS TotalAmount
            INTO #StagingTransactions
            FROM dbo.StagingTransactions s
            INNER JOIN dbo.Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            WHERE s.CompanyId = @CompanyId
                AND s.Version = 0
                AND s.ProductId IS NOT NULL;

            CREATE CLUSTERED INDEX IX_Id ON #StagingTransactions(Id);
            CREATE NONCLUSTERED INDEX IX_ProductId ON #StagingTransactions(ProductId);
            CREATE NONCLUSTERED INDEX IX_TransactionId ON #StagingTransactions(TransactionId);

            -- Insert missing transactions
            INSERT INTO dbo.Transactions
            (
                Id,
                CompanyId,
                TransactionId,
                TransactionDate,
                PaymentMethod,
                Notes,
                TotalAmount
            )
            SELECT
                s.Id,
                s.CompanyId,
                s.TransactionId,
                s.TransactionDate,
                s.PaymentMethod,
                s.Notes,
                s.TotalAmount
            FROM (
                SELECT *,
                    ROW_NUMBER() OVER (PARTITION BY Id ORDER BY (SELECT NULL)) AS rn
                FROM #StagingTransactions
            ) s
            WHERE s.rn = 1
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.Transactions t WHERE t.Id = s.Id
                );

            -- Update existing transactions
            UPDATE t
            SET
                t.TransactionDate = s.TransactionDate,
                t.PaymentMethod   = s.PaymentMethod,
                t.Notes           = s.Notes,
                t.TotalAmount     = s.TotalAmount
            FROM dbo.Transactions t
            INNER JOIN (
                SELECT
                    Id,
                    MAX(TransactionDate) AS TransactionDate,
                    MAX(PaymentMethod)   AS PaymentMethod,
                    MAX(Notes)           AS Notes,
                    SUM(TotalAmount)     AS TotalAmount
                FROM #StagingTransactions
                GROUP BY Id
            ) s ON s.Id = t.Id;

            -- Insert missing line items
            INSERT INTO dbo.TransactionLineItems
            (
                TransactionId,
                ProductId,
                Quantity,
                UnitPrice
            )
            SELECT
                s.Id,
                s.ProductId,
                s.Quantity,
                s.Price
            FROM #StagingTransactions s
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.TransactionLineItems li
                WHERE li.TransactionId = s.Id
                    AND li.ProductId = s.ProductId
            );

            -- Update existing line items
            UPDATE li
            SET
                li.Quantity  = s.Quantity,
                li.UnitPrice = s.Price
            FROM dbo.TransactionLineItems li
            INNER JOIN #StagingTransactions s
                ON li.TransactionId = s.Id
                AND li.ProductId = s.ProductId;

            -- Return affected ProductIds
            SELECT DISTINCT ProductId FROM #StagingTransactions;

            DROP TABLE #StagingTransactions;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
