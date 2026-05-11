using Domain.Entities.Core;
using Domain.Entities.Derived;

namespace Test.Helper;

internal static class ProductHelper
{
    internal static Product CreateProduct(
        Company company,
        string name = "Test Product",
        string sku = "SKU-0001",
        decimal price = 10
    ) => new Product
    {
        Name = name,
        SKU = sku,
        Price = price,
        CompanyId = company.Id,
        Company = company
    };
    

    internal static List<Product> CreateProducts(Company company)
    {
        var product1 = CreateProduct(company: company, name: "Product 1", sku: "SKU-0001");

        product1.InventoryPolicy =InventoryPolicyHelper.CreateInventoryPolicy(
            product: product1,
            safetyStock: 5,
            leadTimeDays: 7,
            minimumOrderQuantity: 5,
            orderIncrementQuantity: 5,
            stockAdjustment: 0
        );

        product1.StockLevel = StockLevelHelper.CreateStockLevel(
            product: product1,
            quantityOnHand: 15,
            quantityOnOrder: 0,
            quantityCommitted: 0,
            netStockPosition: -1,
            forecastQuantity: 0,
            reorderPoint: 1,
            needsReorder: false,
            recommendedOrderQuantity: 2
        );

        var product2 = CreateProduct(company: company, name: "Product 2", sku: "SKU-0002");

        product2.InventoryPolicy = InventoryPolicyHelper.CreateInventoryPolicy(
            product: product2,
            safetyStock: 50,
            leadTimeDays: 7,
            minimumOrderQuantity: 5,
            orderIncrementQuantity: 5,
            stockAdjustment: 0
        );

        product2.StockLevel = StockLevelHelper.CreateStockLevel(
            product: product2,
            quantityOnHand: 25,
            quantityOnOrder: 0,
            quantityCommitted: 0,
            netStockPosition: 10,
            forecastQuantity: 5,
            reorderPoint: 1,
            needsReorder: false,
            recommendedOrderQuantity: 0
        );

        return [product1, product2];
    }
}