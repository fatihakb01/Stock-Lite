using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Context;

public static class DataSeeder
{
    public static async Task SeedAsync(DataContext context)
    {
        if (!context.Companies.Any())
        {
            // Company
            var company = new Company
            {
                Name = "Demo Company",
                Email = "demo@business.com",
                PhoneNumber = "+31612345678",
                Address = "123 Demo street"
            };

            // Users
            var users = new List<User>
            {
                new User {Username = "Superadmin", Email = "superadmin@business.com", PasswordHash = "hashedpassword", Company = company, Role = UserRole.SuperAdmin},
                new User {Username = "Admin", Email = "admin@business.com", PasswordHash = "hashedpassword", Company = company, Role = UserRole.Admin},
                new User {Username = "Keyuser", Email = "keyuser@business.com", PasswordHash = "hashedpassword", Company = company, Role = UserRole.KeyUser},
                new User {Username = "User", Email = "user@business.com", PasswordHash = "hashedpassword", Company = company, Role = UserRole.User}
            };

            // Products
            var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 1200.00m, Company = company },
                new Product { Name = "Mouse", Price = 24.99m, Company = company },
                new Product { Name = "Keyboard", Price = 45.50m, Company = company },
                new Product { Name = "Screen", Price = 100.99m, Company = company },
                new Product { Name = "Chair", Price = 80.97m, Company = company },
                new Product { Name = "Calculator", Price = 19.99m, Company = company },
                new Product { Name = "Charger", Price = 19.99m, Company = company },
                new Product { Name = "Doorbell", Price = 50.10m, Company = company },
                new Product { Name = "Sponge", Price = 4.99m, Company = company },
                new Product { Name = "Chocolate", Price = 0.99m, Company = company }
            };

            // Add random stock levels for each product
            var random = new Random();
            var stockLevels = products.Select(p => new StockLevel
            {
                Product = p,
                Quantity = random.Next(10, 500),       
                ReorderPoint = random.Next(5, 50)      
            }).ToList();

            // Customers
            var customers = new List<Customer>
            {
                new Customer { DisplayName = "Albert Heijn", LegalName = "Albert Heijn B.V.", Email = "info@albertheijn.nl", Company = company },
                new Customer { DisplayName = "Jumbo", LegalName = "Jumbo Supermarkten B.V.", Email = "info@jumbo.nl", Company = company },
                new Customer { DisplayName = "Aldi", LegalName = "Aldi Nederland B.V.", Email = "contact@aldi.nl", Company = company },
                new Customer { DisplayName = "Action", LegalName = "Action Nederland B.V.", Email = "info@action.nl", Company = company },
                new Customer { DisplayName = "Bol.com", LegalName = "Bol.com B.V.", Email = "support@bol.com", Company = company },
                new Customer { DisplayName = "Coolblue", LegalName = "Coolblue B.V.", Email = "service@coolblue.nl", Company = company },
                new Customer { DisplayName = "MediaMarkt", LegalName = "MediaMarkt Nederland B.V.", Email = "contact@mediamarkt.nl", Company = company },
                new Customer { DisplayName = "Hema", LegalName = "Hema B.V.", Email = "info@hema.nl", Company = company },
                new Customer { DisplayName = "Blokker", LegalName = "Blokker B.V.", Email = "service@blokker.nl", Company = company },
                new Customer { DisplayName = "Gamma", LegalName = "Gamma Nederland B.V.", Email = "info@gamma.nl", Company = company }
            };

            // Create random stock levels & re-order points
            // var stockLevels = new List<StockLevel>();
            // var random = new Random();

            // foreach (var product in products)
            // {
            //     var quantity = random.Next(0, 500);       
            //     var reorderPoint = random.Next(20, 100);  

            //     var stock = new StockLevel
            //     {
            //         Product = product,   
            //         Quantity = quantity,
            //         ReorderPoint = reorderPoint
            //     };

            //     stockLevels.Add(stock);
            // }

            // Transactions
            var transactions = new List<Transaction>();
            var lineItems = new List<TransactionLineItem>();

            // Create random transactions
            int numberOfRandomTransactions = 100;

            for (int i = 0; i < numberOfRandomTransactions; i++)
            {
                // Pick a random customer
                var customer = customers[random.Next(customers.Count)];

                // Pick a few random products
                var selectedProducts = products.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

                // Create a transaction
                var transaction = new Transaction
                {
                    Customer = customer,
                    TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    PaymentMethod = random.Next(2) == 0 ? "Credit Card" : "Cash",
                    Notes = random.Next(2) == 0 ? null : "Thank you for your purchase!"
                };

                // Determine the total quantity of a transaction
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

                // Add total amount to transactions
                transaction.TotalAmount = total;
                transactions.Add(transaction);
            }

            context.Companies.Add(company);
            context.Users.AddRange(users);
            context.Products.AddRange(products);
            context.StockLevels.AddRange(stockLevels);
            context.Customers.AddRange(customers);
            context.Transactions.AddRange(transactions);
            context.TransactionLineItems.AddRange(lineItems);

            await context.SaveChangesAsync();
        }
    }
}
