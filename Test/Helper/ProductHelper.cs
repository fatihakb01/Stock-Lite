using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper;

internal class ProductHelper(DatabaseFixture fixture)
{
    private readonly DatabaseFixture _fixture = fixture;

    internal static Product CreateProductStatic(
        Company company,
        string name = "Test Product",
        string sku = "SKU-0001",
        decimal price = 10
    )
    {
        return new Product
        {
            Name = name,
            SKU = sku,
            Price = price,
            CompanyId = company.Id,
            Company = company
        };
    }

    internal async Task<Product> CreateProduct(
        Company company,
        string name = "Test Product",
        string sku = "SKU-0001",
        decimal price = 10,
        bool saveChanges = true
    )
    {
        var product = CreateProductStatic(company, name, sku, price);

        await _fixture.Context.Products.AddAsync(product);

        if (saveChanges)
            await _fixture.Context.SaveChangesAsync();

        return product;
    }

    internal async Task<List<Product>> CreateProducts(Company company)
    {
        var product1 = await CreateProduct(
            company: company, 
            name: "Product 1", 
            sku: "SKU-0001",
            saveChanges: false
         );

        product1.InventoryPolicy = InventoryPolicyHelper.CreateInventoryPolicyStatic(
            product: product1,
            safetyStock: 5,
            leadTimeDays: 7,
            minimumOrderQuantity: 5,
            orderIncrementQuantity: 5,
            stockAdjustment: 0
        );

        product1.StockLevel = StockLevelHelper.CreateStockLevelStatic(
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

        var product2 = await CreateProduct(
            company: company, 
            name: "Product 2", 
            sku: "SKU-0002",
            saveChanges: false
         );

        product2.InventoryPolicy = InventoryPolicyHelper.CreateInventoryPolicyStatic(
            product: product2,
            safetyStock: 50,
            leadTimeDays: 7,
            minimumOrderQuantity: 5,
            orderIncrementQuantity: 5,
            stockAdjustment: 0
        );

        product2.StockLevel = StockLevelHelper.CreateStockLevelStatic(
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

        await _fixture.Context.Products.AddRangeAsync(product1, product2);
        await _fixture.Context.SaveChangesAsync();

        return [product1, product2];
    }

    private static string BuildCsvHeaders()
    {
        return
            "SKU;Name;Price;Description;IsActive;LeadTimeDays;MinimumOrderQuantity;OrderIncrementQuantity;SafetyStock;StockAdjustment\r\n";
    }

    internal static string BuildCsv(
        string sku = "SKU-0001",
        string name = "Product Test",
        decimal price = 10m,
        string description = "",
        bool isActive = true,
        int leadTimeDays = 7,
        int moq = 1,
        int ioq = 1,
        int safetyStock = 1,
        int stockAdjustment = 0,
        bool includeHeaders = true
        )
    {
        string content = $"{sku};{name};{price};{description};{isActive};{leadTimeDays};{moq};{ioq};{safetyStock};{stockAdjustment}";
        if (includeHeaders)
        {
            return BuildCsvHeaders() + $"{content}\r\n";
        }

        return $"{content}\r\n";
    }

    internal async Task<int> CountProducts(Guid companyId)
    {
        return await _fixture.Context.Products
            .CountAsync(p => p.CompanyId == companyId);
    }

    internal async Task<Product?> GetProduct(
        Guid companyId,
        string sku)
    {
        return await _fixture.Context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.CompanyId == companyId &&
                x.SKU == sku);
    }
}