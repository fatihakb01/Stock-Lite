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
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                TransactionId NVARCHAR(50) NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                TransactionDate DATE NOT NULL,
                PaymentMethod NVARCHAR(50) NULL,
                Notes NVARCHAR(200) NULL,
                Quantity INT NOT NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessTransactions exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE dbo.ProcessTransactions
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Insert missing transactions
            INSERT INTO Transactions
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
                NEWID(),
                s.CompanyId,
                s.TransactionId,
                MIN(s.TransactionDate),
                MIN(ISNULL(s.PaymentMethod, '')),
                MIN(ISNULL(s.Notes, '')),
                SUM(s.Quantity * p.Price)
            FROM dbo.StagingTransactions s
            JOIN Products p
                ON p.SKU = s.SKU
            AND p.CompanyId = s.CompanyId
            WHERE s.CompanyId = @CompanyId
            AND NOT EXISTS (
                SELECT 1
                FROM Transactions t
                WHERE t.CompanyId = s.CompanyId
                    AND t.TransactionId = s.TransactionId
            )
            GROUP BY s.CompanyId, s.TransactionId;

            -- Update transactions
            UPDATE t
            SET
                t.TransactionDate = s.TransactionDate,
                t.PaymentMethod = s.PaymentMethod,
                t.Notes = s.Notes,
                t.TotalAmount = s.TotalAmount
            FROM Transactions t
            JOIN (
                SELECT
                    s.CompanyId,
                    TransactionId,
                    MIN(TransactionDate) AS TransactionDate,
                    MIN(ISNULL(PaymentMethod, '')) AS PaymentMethod,
                    MIN(ISNULL(Notes, '')) AS Notes,
                    SUM(Quantity * p.Price) AS TotalAmount
                FROM dbo.StagingTransactions s
                JOIN Products p
                    ON p.SKU = s.SKU
                    AND p.CompanyId = s.CompanyId
                WHERE s.CompanyId = @CompanyId
                GROUP BY s.CompanyId, TransactionId
            ) s ON s.CompanyId = t.CompanyId
            AND s.TransactionId = t.TransactionId;

            -- Insert missing line items
            INSERT INTO TransactionLineItems
            (
                TransactionId,
                ProductId,
                Quantity,
                UnitPrice
            )
            SELECT
                t.Id,
                p.Id,
                s.Quantity,
                p.Price
            FROM dbo.StagingTransactions s
            JOIN Products p
                ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
            JOIN Transactions t
                ON t.TransactionId = s.TransactionId
                AND t.CompanyId = s.CompanyId
            LEFT JOIN TransactionLineItems li
                ON li.TransactionId = t.Id
                AND li.ProductId = p.Id
            WHERE s.CompanyId = @CompanyId
            AND li.ProductId IS NULL;

            -- Update line items
            UPDATE li
            SET
                li.Quantity = s.Quantity,
                li.UnitPrice = p.Price
            FROM TransactionLineItems li
            JOIN Transactions t ON t.Id = li.TransactionId
            JOIN Products p ON p.Id = li.ProductId
            JOIN dbo.StagingTransactions s
                ON s.TransactionId = t.TransactionId
                AND s.SKU = p.SKU
                AND s.CompanyId = t.CompanyId
            WHERE t.CompanyId = @CompanyId;

            DELETE FROM dbo.StagingTransactions WHERE CompanyId = @CompanyId;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
