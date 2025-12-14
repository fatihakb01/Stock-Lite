using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer;

public static class UserDefinedTypesInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        await EnsureGuidListTableAsync(context, logger);
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
}
