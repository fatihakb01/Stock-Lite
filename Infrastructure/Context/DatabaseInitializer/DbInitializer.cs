using Infrastructure.Context.DatabaseInitializer.Scheduler;
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
            await SeasonalityInitializer.InitializeAsync(context, logger);
            await ForecastsInitializer.InitializeAsync(context, logger);          
            await ConfirmedOrderAggregatesInitializer.InitializeAsync(context, logger); 
            await PurchaseForecastInitializer.InitializeAsync(context, logger);
            await StockLevelInitializer.InitializeAsync(context, logger);   
            await SchedulerImportInitializer.InitializeAsync(context, logger);
            await DataSeederInitializer.InitializeAsync(context, logger);
        }
    }
}
