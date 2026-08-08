using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class UserDefinedTypesInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureGuidListTableAsync(context, logger);
        await EnsureStagingTableMappingAsync(context, logger);
        await EnsureRefreshStagingTable(context, logger);
    }

    private static async Task EnsureGuidListTableAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring guid list table exist...");

        var sql = """
        -- Ensure the GuidList table type exists
        IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'GuidList')
        BEGIN
            CREATE TYPE dbo.GuidList AS TABLE (Id UNIQUEIDENTIFIER);
        END
        """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureStagingTableMappingAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.StagingTableMapping exists...");

        var sql = """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StagingTableMapping')
        BEGIN
            CREATE TABLE dbo.StagingTableMapping (
                ImportDataType INT PRIMARY KEY,
                TableName NVARCHAR(128) NOT NULL,
                ProcedureName NVARCHAR(128) NOT NULL
            );

            INSERT INTO dbo.StagingTableMapping (ImportDataType, TableName, ProcedureName) VALUES
            (0, 'StagingProducts', 'ProcessProducts'),
            (1, 'StagingTransactions', 'ProcessTransactions'),
            (2, 'StagingConfirmedOrders', 'ProcessConfirmedOrders'),
            (3, 'StagingPurchaseOrders', 'ProcessPurchaseOrders'),
            (4, 'StagingUserDefinedFields', 'ProcessUserDefinedFields');
        END
        """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task EnsureRefreshStagingTable(DataContext context, ILogger logger)
    {
        logger.LogInformation("Ensuring dbo.RefreshStagingTable exists...");

        var sql = @"
        CREATE OR ALTER PROCEDURE [dbo].[RefreshStagingTable]
            @CompanyId UNIQUEIDENTIFIER,
            @ImportDataType INT,
            @MaximumVersions INT = 3
        AS
        BEGIN
            SET NOCOUNT ON;
    
            DECLARE @TableName NVARCHAR(128);
            DECLARE @Sql NVARCHAR(MAX);
    
            SELECT @TableName = TableName 
            FROM dbo.StagingTableMapping 
            WHERE ImportDataType = @ImportDataType;
    
            IF @TableName IS NULL
            BEGIN
                RAISERROR('Invalid ImportDataType: %d', 16, 1, @ImportDataType);
                RETURN;
            END
    
            SET @Sql = N'
                UPDATE dbo.' + QUOTENAME(@TableName) + N'
                SET [Version] += 1;
        
                DELETE 
                FROM dbo.' + QUOTENAME(@TableName) + N'
                WHERE CompanyId = @CompanyId
                AND [Version] > @MaximumVersions;';
    
            EXEC sp_executesql @Sql,
                N'@CompanyId UNIQUEIDENTIFIER, @MaximumVersions INT',
                @CompanyId, @MaximumVersions;
        END
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
