using System.Globalization;
using CsvHelper;
using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Context.SeedData.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context.SeedData.Core;

public class InitialData
{
    public static async Task SeedAsync(DataContext context, IConfiguration config)
    {
        
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

        var policies = products.Select(p =>
        {
            return new ProductInventoryPolicy
            {
                ProductId = p.Id,
                Product = null!,

                LeadTimeDays = random.Next(3, 30),

                MinimumOrderQuantity = random.Next(10, 100),
                OrderIncrementQuantity = random.Next(1, 10),

                SafetyStock = random.Next(5, 50),

                StockAdjustment = random.Next(-10, 10),

                
            };
        }).ToList();

        // Save entities to database
        context.Companies.Add(company);
        context.Users.AddRange(users);
        context.Products.AddRange(products);
        context.ProductInventoryPolicies.AddRange(policies);
        await context.SaveChangesAsync();
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
