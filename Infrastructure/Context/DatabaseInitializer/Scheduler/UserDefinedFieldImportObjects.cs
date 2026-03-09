using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

public class UserDefinedFieldImportObjects
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureStagingTableAsync(context, logger);
        await EnsureProcessProcAsync(context, logger);
    }

    private static async Task EnsureStagingTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingUserDefinedFields exists...");

        var sql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingUserDefinedFields')
        BEGIN
            CREATE TABLE dbo.StagingUserDefinedFields
            (
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                [Key] NVARCHAR(50) NOT NULL,
                [Value] NVARCHAR(200) NULL
            );
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureProcessProcAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.ProcessUserDefinedFields exists...");

        var sql = @"
		CREATE OR ALTER PROCEDURE dbo.ProcessUserDefinedFields
            @CompanyId UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Insert new keys for products
            INSERT INTO UserDefinedFields (
                [ProductId],
                [Key],
                [Value],
                CompanyId
            )
            SELECT
                p.Id,
                [Key],
                [Value],
                @CompanyId
            FROM StagingUserDefinedFields s
            INNER JOIN Products p ON p.SKU = s.SKU
            WHERE NOT EXISTS (
                SELECT 1
                FROM UserDefinedFields u
                WHERE u.ProductId = p.Id
                AND u.CompanyId = @CompanyId
                AND u.[Key] = s.[Key]
            );

            -- Update existing keys for products
            UPDATE u
            SET u.[Key] = s.[Key],
                u.[Value] = s.[Value]
            FROM StagingUserDefinedFields s
            INNER JOIN Products p 
                ON p.SKU = s.SKU
            INNER JOIN UserDefinedFields u 
                ON u.ProductId = p.Id
                AND u.CompanyId = @CompanyId
                AND u.[Key] = s.[Key];

            -- Reset staging table
            DELETE FROM dbo.StagingUserDefinedFields WHERE CompanyId = @CompanyId;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
