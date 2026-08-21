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
                Version INT NOT NULL,
                ImportTimeStamp DATETIME NOT NULL,
                CompanyId UNIQUEIDENTIFIER NOT NULL,
                SKU NVARCHAR(50) NOT NULL,
                [Key] NVARCHAR(50) NOT NULL,
                [Value] NVARCHAR(200) NULL,
                ProductId UNIQUEIDENTIFIER NULL
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

			-- Update ProductId in staging table
			UPDATE s
			SET s.ProductId = p.Id
			FROM dbo.StagingUserDefinedFields s
			INNER JOIN dbo.Products p
				ON p.SKU = s.SKU
                AND p.CompanyId = s.CompanyId
			WHERE p.CompanyId = @CompanyId
            AND s.Version = 0;

			-- Create staging temp table with all necessary data
			SELECT
				Version,
				ImportTimeStamp,
                ProductId,
                SKU,
                [Key],
                [Value],
                CompanyId
			INTO #StagingUserDefinedFields
			FROM dbo.StagingUserDefinedFields s
			WHERE s.CompanyId = @CompanyId
			AND s.Version = 0
			AND s.ProductId IS NOT NULL;

            CREATE CLUSTERED INDEX IX_Id ON #StagingUserDefinedFields(ProductId, CompanyId, [Key]);

            -- Insert new keys for products
            INSERT INTO UserDefinedFields (
                ProductId,
                [Key],
                [Value],
                CompanyId
            )
            SELECT
                s.ProductId,
                [Key],
                [Value],
                s.CompanyId
            FROM #StagingUserDefinedFields s
            WHERE NOT EXISTS (
                SELECT 1
                FROM UserDefinedFields u
                WHERE u.ProductId = s.ProductId
                AND u.CompanyId = s.CompanyId
                AND u.[Key] = s.[Key]
            );

            -- Update existing keys for products
            UPDATE u
            SET u.[Key] = s.[Key],
                u.[Value] = s.[Value]
            FROM #StagingUserDefinedFields s
            INNER JOIN UserDefinedFields u 
                ON u.ProductId = s.ProductId
                AND u.CompanyId = s.CompanyId
                AND u.[Key] = s.[Key];

            -- Return affected ProductIds
			SELECT DISTINCT ProductId FROM #StagingUserDefinedFields;

			DROP TABLE #StagingUserDefinedFields;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
