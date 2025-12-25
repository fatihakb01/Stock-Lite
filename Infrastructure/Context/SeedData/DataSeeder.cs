using Domain.Entities.Core;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context.SeedData;

public static class DataSeeder
{
    private const int BatchSize = 10_000;
    private const int TotalTransactions = 1_000_000;

    public static async Task SeedAsync(DataContext context, IConfiguration config)
    {
        if (await context.Companies.AsNoTracking().AnyAsync())
            return;

        var random = new Random();

        // Company
        var company = new Company
        {
            Name = "Demo Company",
            Email = "demo@business.com",
            PhoneNumber = "+31612345678",
            Address = "123 Demo Street"
        };

        // Users
        var users = new List<User>
        {
            new() { UserName = "Superadmin", Email = "superadmin@business.com", Company = company, Role = UserRole.SuperAdmin },
            new() { UserName = "Admin", Email = "admin@business.com", Company = company, Role = UserRole.Admin },
            new() { UserName = "Keyuser", Email = "keyuser@business.com", Company = company, Role = UserRole.KeyUser },
            new() { UserName = "User", Email = "user@business.com", Company = company, Role = UserRole.User }
        };

        var passwordHasher = new PasswordHasher<User>();
        string defaultPassword = "Password123!"; 

        foreach (var user in users)
        {
            user.NormalizedEmail = user.Email?.ToUpperInvariant();
            user.NormalizedUserName = user.UserName?.ToUpperInvariant();

            if (string.IsNullOrEmpty(user.SecurityStamp))
                user.SecurityStamp = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(user.ConcurrencyStamp))
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

            user.EmailConfirmed = true;
            user.IsActive = true;

            user.PasswordHash = passwordHasher.HashPassword(user, defaultPassword);
        }

        // Product names with simple descriptions
        var productDefinitions = new Dictionary<string, string>();

        var productPath = config["SeedData:Product"]!;
        productDefinitions = LoadProductsFromCsv(productPath);

        var products = productDefinitions.Select((pd, index) => new Product
        {
            Name = pd.Key,
            Description = pd.Value,
            SKU = $"PRD-{(index + 1).ToString("D4")}",
            Price = Math.Round((decimal)(random.NextDouble() * 500 + 5), 2),
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 1000)),
            IsActive = random.Next(10) > 1, // 90% active
            Company = company
        }).ToList();

        // Stock Levels
        var stockLevels = products.Select(p => new StockLevel
        {
            Product = p,
            QuantityOnHand = random.Next(10, 500),
            ReorderPoint = random.Next(5, 50)
        }).ToList();

        // Save entities (except for transactions) to database
        context.Companies.Add(company);
        context.Users.AddRange(users);
        context.Products.AddRange(products);
        context.StockLevels.AddRange(stockLevels);
        await context.SaveChangesAsync();

        // Transactions
        var transactions = new List<Transaction>();
        var lineItems = new List<TransactionLineItem>();

        // Initialize batch number and total batches
        int batchNr = 0;
        double totalBatches = TotalTransactions % BatchSize > 0 ?
            (TotalTransactions / BatchSize) + 1 :
            TotalTransactions / BatchSize;

        for (int i = 0; i < TotalTransactions; i++)
        {
            var selectedProducts = products.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

            var transaction = new Transaction
            {
                TransactionId = "T" + i,
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
                var lineItem = new TransactionLineItem
                {
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Transaction = transaction,
                    Product = product
                };
                total += product.Price * quantity;
                lineItems.Add(lineItem);
            }

            transaction.TotalAmount = total;
            transactions.Add(transaction);

            if (transactions.Count % BatchSize == 0)
            {
                await context.Transactions.AddRangeAsync(transactions);
                await context.TransactionLineItems.AddRangeAsync(lineItems);
                await context.SaveChangesAsync();
                transactions.Clear();
                lineItems.Clear();

                Console.WriteLine($"Batch {++batchNr}/{totalBatches} completed.");
            }
        }
    }
    
    private static Dictionary<string, string> LoadProductsFromCsv(string path)
    {
        var productDefinitions = new Dictionary<string, string>();

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        });

        foreach (var record in csv.GetRecords<ProductCsvRecord>())
        {
            if (!string.IsNullOrWhiteSpace(record.Name))
                productDefinitions[record.Name] = record.Description ?? string.Empty;
        }

        Console.WriteLine($"Loaded {productDefinitions.Count} unique names.");

        return productDefinitions;
    }
}
