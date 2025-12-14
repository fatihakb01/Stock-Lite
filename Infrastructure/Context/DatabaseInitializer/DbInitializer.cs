using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DataContext context, ILogger logger)
        {
            await context.Database.MigrateAsync();

            await UserDefinedTypesInitializer.InitializeAsync(context, logger);

            await SalesAggregatesInitializer.InitializeAsync(context, logger);
            
            await ForecastsInitializer.InitializeAsync(context, logger);           
        }
    }
}
