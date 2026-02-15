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
    public async Task<ProductKpi> GetProductKpis(Guid companyId, CancellationToken ct)
    {
        var schedulerFailed = await FailedScheduledJobsPresent(companyId, ct);
        var totalProducts = await GetProductsCount(companyId, ct);
        var belowSafetyStock = await GetProductsBelowSafetyStockCount(companyId, ct);
        var recommendedOrders = await GetProductsWithRecommendedOrdersCount(companyId, ct);
        var withoutStockAtLeadTime = await GetProductsWithoutStockAtLeadTimeCount(companyId, ct);
        var withoutForecast = await GetProductsWithoutForecastCount(companyId, ct);
        var company = await _context.Companies.SingleOrDefaultAsync(c => c.Id == companyId, ct);

        return new ProductKpi()
        {
            SchedulerFailed = schedulerFailed,
            TotalProductCount = totalProducts,
            BelowSafetyStockCount = belowSafetyStock,
            RecommendedOrdersCount = recommendedOrders,
            WithoutStockAtLeadTimeCount = withoutStockAtLeadTime,
            WithoutForecastCount = withoutForecast,
            CompanyId = companyId,
            Company = company!
        };
    }
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

    private async Task<bool> FailedScheduledJobsPresent(Guid companyId, CancellationToken ct)
    {
        return await _context.ScheduledJobs
            .Include(s => s.JobTask)
            .Where(s => 
                s.JobTask.CompanyId == companyId && 
                s.Result == JobRunResult.Failed
            )
            .AnyAsync(ct);
    }

    private async Task<List<Product>> GetProducts(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .ToListAsync(ct);
    }

    private async Task<int> GetProductsCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .CountAsync(ct);
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

    private async Task<int> GetProductsBelowSafetyStockCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.QuantityOnHand < p.InventoryPolicy!.SafetyStock
            )
            .CountAsync(ct);
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

    private async Task<int> GetProductsWithoutStockAtLeadTimeCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.NetStockPosition <= 0
            )
            .CountAsync(ct);
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

    private async Task<int> GetProductsWithoutForecastCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.ForecastQuantity <= 0
            )
            .CountAsync(ct);
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

    private async Task<int> GetProductsWithRecommendedOrdersCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.RecommendedOrderQuantity > 0
            )
            .CountAsync(ct);
    }
}
