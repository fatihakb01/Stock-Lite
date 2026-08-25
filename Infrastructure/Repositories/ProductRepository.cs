using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository(DataContext context)
    : Repository<Product>(context), IProductRepository
{
    private readonly DataContext _context = context;
    public async Task<List<Product>> GetFilteredProducts(
        ProductFilter? filter, 
        Guid companyId, 
        CancellationToken ct
    )
    {
        return filter switch
        {
            ProductFilter.BelowSafetyStock =>
                await GetProductsBelowSafetyStock(companyId, ct),
            ProductFilter.WithRecommendedOrders =>
                await GetProductsWithRecommendedOrders(companyId, ct),
            ProductFilter.WithoutStockAtLeadTime =>
                await GetProductsWithoutStockAtLeadTime(companyId, ct),
            ProductFilter.WithoutForecast =>
                await GetProductsWithoutForecast(companyId, ct),
            _ =>
                await GetProducts(companyId, ct)
        };
    }

    public async Task<List<Guid>> GetProductIds(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .Select(p => p.Id)
            .ToListAsync(ct);
    }

    private async Task<List<Product>> GetProducts(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .ToListAsync(ct);
    }

    private async Task<List<Product>> GetProductsBelowSafetyStock(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Include(p => p.InventoryPolicy)
            .Include(p => p.StockLevel)
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.QuantityOnHand < p.InventoryPolicy!.SafetyStock
            )
            .ToListAsync(ct);
    }

    private async Task<List<Product>> GetProductsWithoutStockAtLeadTime(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Include(p => p.StockLevel)
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.NetStockPosition <= 0
            )
            .ToListAsync(ct);
    }

    private async Task<List<Product>> GetProductsWithoutForecast(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Include(p => p.StockLevel)
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.ForecastQuantity <= 0
            )
            .ToListAsync(ct);
    }

    private async Task<List<Product>> GetProductsWithRecommendedOrders(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Include(p => p.StockLevel)
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.RecommendedOrderQuantity > 0
            )
            .ToListAsync(ct);
    }
}
