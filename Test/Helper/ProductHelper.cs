using Domain.Entities.Core;
using Domain.Entities.Derived;

namespace Test.Helper;

internal static class ProductHelper
{
    internal static Product CreateProduct(
        Company company,
        string name = "Test Product",
        string sku = "SKU-0001",
        decimal price = 9.99m,
        int quantityOnHand = 10,
        int netStockPosition = 10,
        int forecastQuantity = 10,
        int recommendedOrderQuantity = 0,
        int safetyStock = 5)
    {
        var product = new Product
        {
            Name = name,
            SKU = sku,
            Price = price,
            CompanyId = company.Id,
            Company = company
        };

        var stockLevel = new StockLevel
        {
            ProductId = product.Id,
            Product = product,
            QuantityOnHand = quantityOnHand,
            QuantityOnOrder = 0,
            QuantityCommitted = 0,
            NetStockPosition = netStockPosition,
            ForecastQuantity = forecastQuantity,
            ReorderPoint = 1,
            NeedsReorder = false,
            RecommendedOrderQuantity = recommendedOrderQuantity,
            LastCalculatedAt = DateTime.UtcNow
        };

        var inventoryPolicy = new ProductInventoryPolicy
        {
            ProductId = product.Id,
            Product = product,
            LeadTimeDays = 10,
            MinimumOrderQuantity = 1,
            OrderIncrementQuantity = 1,
            SafetyStock = safetyStock,
            StockAdjustment = 0
        };

        product.StockLevel = stockLevel;
        product.InventoryPolicy = inventoryPolicy;

        return product;
    }

    internal static List<Product> CreateProducts(Company company)
    {
        return
        [
            CreateProduct(
                company,
                name: "Product 1",
                sku: "SKU-0001",
                quantityOnHand: 15,
                netStockPosition: -1,
                forecastQuantity: 0,
                recommendedOrderQuantity: 2,
                safetyStock: 5),

            CreateProduct(
                company,
                name: "Product 2",
                sku: "SKU-0002",
                quantityOnHand: 25,
                netStockPosition: 10,
                forecastQuantity: 5,
                recommendedOrderQuantity: 0,
                safetyStock: 50)
        ];
    }
}