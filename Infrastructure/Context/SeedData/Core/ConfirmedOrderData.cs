using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Context.SeedData.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context.SeedData.Core;

public class ConfirmedOrderData
{
    private const int BatchSize = 10_000;
    private const int TotalCOs = 100_000;

    public static async Task SeedAsync(DataContext context, SeedStockContext stock)
    {
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var random = new Random();

        var company = await context.Companies
            .SingleAsync(c => c.Name == "Demo Company");

        var products = await context.Products
            .Where(p => p.CompanyId == company.Id)
            .ToListAsync();

        if (products.Count == 0)
            throw new InvalidOperationException("Demo Company has no products.");

        var cos = new List<ConfirmedOrder>(BatchSize);
        var coLineItems = new List<ConfirmedOrderLineItem>(BatchSize * 3);

        var totalBatches = (int)Math.Ceiling((double)TotalCOs / BatchSize);
        var batchNr = 0;

        for (int i = 0; i < TotalCOs; i++)
        {
            var status = (ConfirmedOrderStatus)random.Next(
                0,
                Enum.GetValues<ConfirmedOrderStatus>().Length
            );

            var orderDate = DateTime.UtcNow.AddDays(random.Next(1, 365));
            var expectedDeliveryDate = orderDate.AddDays(random.Next(3, 30));

            var co = new ConfirmedOrder
            {
                OrderNumber = $"CO-{i:D7}",
                Status = status,
                OrderDate = orderDate,
                ExpectedDeliveryDate = expectedDeliveryDate,
                ReceivedDate = status == ConfirmedOrderStatus.Fulfilled
                    ? expectedDeliveryDate.AddDays(random.Next(-1, 2))
                    : null,
                CompanyId = company.Id,
                Company = company
            };

            var selectedProducts = products
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, 4))
                .ToList();

            foreach (var product in selectedProducts)
            {
                var orderedQty = random.Next(5, 10);

                var receivedQty =
                    status == ConfirmedOrderStatus.Fulfilled
                        ? orderedQty
                        : status == ConfirmedOrderStatus.PartiallyFulfilled
                            ? random.Next(1, orderedQty)
                            : 0;

                coLineItems.Add(new ConfirmedOrderLineItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = orderedQty,
                    SoldQuantity = receivedQty,
                    UnitCost = product.Price,
                    ConfirmedOrder = co
                });

                var availableToCommit =
                    stock.OnHand[product.Id] - stock.Committed.GetValueOrDefault(product.Id);

                var commitQty = Math.Min(orderedQty, availableToCommit);

                stock.Committed[product.Id] += commitQty;
            }

            cos.Add(co);

            if (cos.Count == BatchSize)
            {
                await context.ConfirmedOrder.AddRangeAsync(cos);
                await context.ConfirmedOrderLineItem.AddRangeAsync(coLineItems);
                await context.SaveChangesAsync();

                cos.Clear();
                coLineItems.Clear();

                Console.WriteLine($"Confirmed Order Batch {++batchNr}/{totalBatches} completed.");
            }
        }

        // Flush remaining
        if (cos.Count != 0)
        {
            await context.ConfirmedOrder.AddRangeAsync(cos);
            await context.ConfirmedOrderLineItem.AddRangeAsync(coLineItems);
            await context.SaveChangesAsync();
        }

        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
