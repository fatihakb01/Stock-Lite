using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Context.SeedData.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context.SeedData.Core;

public static class PurchaseOrderData
{
    private const int BatchSize = 10_000;
    private const int TotalPOs = 100_000;

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

        var pos = new List<PurchaseOrder>(BatchSize);
        var poLineItems = new List<PurchaseOrderLineItem>(BatchSize * 3);

        var totalBatches = (int)Math.Ceiling((double)TotalPOs / BatchSize);
        var batchNr = 0;

        for (int i = 0; i < TotalPOs; i++)
        {
            // var status = (PurchaseOrderStatus)random.Next(
            //     0,
            //     Enum.GetValues<PurchaseOrderStatus>().Length
            // );
            var status = PurchaseOrderStatus.Completed;

            var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 60));
            var expectedDeliveryDate = orderDate.AddDays(random.Next(3, 30));

            var po = new PurchaseOrder
            {
                OrderNumber = $"PO-{i:D7}",
                Status = status,
                OrderDate = orderDate,
                ExpectedDeliveryDate = expectedDeliveryDate,
                ReceivedDate = status == PurchaseOrderStatus.Completed
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
                    status == PurchaseOrderStatus.Completed
                        ? orderedQty
                        : status == PurchaseOrderStatus.PartiallyReceived
                            ? random.Next(1, orderedQty)
                            : 0;

                poLineItems.Add(new PurchaseOrderLineItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = orderedQty,
                    ReceivedQuantity = receivedQty,
                    UnitCost = product.Price,
                    PurchaseOrder = po
                });

                stock.OnHand[product.Id] += receivedQty;
                stock.OnOrder[product.Id] += orderedQty - receivedQty;
            }

            pos.Add(po);

            if (pos.Count == BatchSize)
            {
                await context.PurchaseOrder.AddRangeAsync(pos);
                await context.PurchaseOrderLineItem.AddRangeAsync(poLineItems);
                await context.SaveChangesAsync();

                pos.Clear();
                poLineItems.Clear();

                Console.WriteLine($"Purchase Order Batch {++batchNr}/{totalBatches} completed.");
            }
        }

        // Flush remaining
        if (pos.Count != 0)
        {
            await context.PurchaseOrder.AddRangeAsync(pos);
            await context.PurchaseOrderLineItem.AddRangeAsync(poLineItems);
            await context.SaveChangesAsync();
        }

        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}