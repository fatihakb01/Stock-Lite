using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            // Apply any pending migrations first
            await context.Database.MigrateAsync();

            // Initialize sales aggregates
            await SalesAggregatesInitializer.InitializeAsync(context, logger);

            // Initialize forecasts
            await ForecastsInitializer.InitializeAsync(context, logger);           
        }
    }
}
