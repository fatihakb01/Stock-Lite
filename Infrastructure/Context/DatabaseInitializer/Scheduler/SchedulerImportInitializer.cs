using Microsoft.Extensions.Logging;

namespace Infrastructure.Context.DatabaseInitializer.Scheduler;

public class SchedulerImportInitializer
{
    public static async Task InitializeAsync(DataContext context, ILogger logger)
    {
        logger.LogInformation("Initializing scheduler import objects...");

        await ConfirmedOrderImportObjects.InitializeAsync(context, logger);
        await ProductImportObjects.InitializeAsync(context, logger);
        await PurchaseOrderImportObjects.InitializeAsync(context, logger);
        await TransactionImportObjects.InitializeAsync(context, logger);
        await UserDefinedFieldImportObjects.InitializeAsync(context, logger);
    }
}
