using Domain.Entities.Core;
using Infrastructure.Context.SeedData.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context.SeedData.Core;

public static class TransactionsData
{
    private const int BatchSize = 10_000;
    private const int TotalTransactions = 100_000;

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

        var transactions = new List<Transaction>(BatchSize);
        var transactionLineItems = new List<TransactionLineItem>(BatchSize * 3);

        var totalBatches = (int)Math.Ceiling((double)TotalTransactions / BatchSize);
        var batchNr = 0;

        for (int i = 0; i < TotalTransactions; i++)
        {
            var selectedProducts = products.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

            var transaction = new Transaction
            {
                TransactionId = $"T-{i:D8}",
                TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                PaymentMethod = random.Next(2) == 0 ? "Credit Card" : "Cash",
                Notes = random.Next(3) switch
                {
                    0 => null,
                    1 => "Thank you for your purchase!",
                    _ => "Processed by cashier #" + random.Next(1, 10)
                },
                Company = company
            };

            decimal total = 0;
            foreach (var product in selectedProducts)
            {
                var quantity = random.Next(1, 5);

                var sellableQty = Math.Min(quantity, stock.OnHand[product.Id]);

                if (sellableQty == 0)
                    continue;

                stock.OnHand[product.Id] -= sellableQty;

                var lineItem = new TransactionLineItem
                {
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Transaction = transaction,
                    Product = product
                };
                
                total += product.Price * quantity;
                transactionLineItems.Add(lineItem);
            }

            transaction.TotalAmount = total;
            transactions.Add(transaction);

            if (transactions.Count % BatchSize == 0)
            {
                await context.Transactions.AddRangeAsync(transactions);
                await context.TransactionLineItems.AddRangeAsync(transactionLineItems);
                await context.SaveChangesAsync();

                transactions.Clear();
                transactionLineItems.Clear();

                Console.WriteLine($"Transaction Batch {++batchNr}/{totalBatches} completed.");
            }
        }

        // Delete transactions that don't have transaction line items
        await context.Database.ExecuteSqlRawAsync(@"
			DELETE 
			FROM Transactions 
			WHERE NOT EXISTS (
				SELECT 1
				FROM TransactionLineItems
				WHERE TransactionId = Id
			)
        ");
    }
}
