using Infrastructure.Context.SeedData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context.SeedData.Core;

public static class DataSeeder
{
    public static async Task SeedAsync(DataContext context, IConfiguration config)
    {
        if (await context.Companies.AsNoTracking().AnyAsync())
            return;

        await InitialData.SeedAsync(context, config);

        var stock = new SeedStockContext();

        await InitialInventoryData.SeedAsync(context, stock);
        await PurchaseOrderData.SeedAsync(context, stock);
        await ConfirmedOrderData.SeedAsync(context, stock);
        await TransactionsData.SeedAsync(context, stock);
    }
}
