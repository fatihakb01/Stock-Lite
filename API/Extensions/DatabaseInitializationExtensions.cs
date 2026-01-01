using Infrastructure.Context;
using Infrastructure.Context.DatabaseInitializer;
using Infrastructure.Context.SeedData.Core;

namespace API.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeAndSeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DataContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var config = services.GetRequiredService<IConfiguration>();

        try
        {
            await DbInitializer.InitializeAsync(context, logger);
            logger.LogInformation("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
        }

        if (app.Environment.IsDevelopment())
        {
            await DataSeeder.SeedAsync(context, config);
        }
    }
}
