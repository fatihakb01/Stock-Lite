using Infrastructure.Context.SeedData.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context.SeedData.Core;

public static class InitialInventoryData
{
    public static async Task SeedAsync(DataContext context, SeedStockContext stock)
    {
        var company = await context.Companies.SingleAsync(c => c.Name == "Demo Company");
        var products = await context.Products.Where(p => p.CompanyId == company.Id).ToListAsync();

        var random = new Random();
        int count = 0;

        foreach (var product in products)
        {
            count++;

            stock.OnHand[product.Id] = 0;
            stock.OnOrder[product.Id] = 0;
            stock.Committed[product.Id] = 0;

            Console.WriteLine($"{count}/{products.Count} products completed.");
        }
    }
}
