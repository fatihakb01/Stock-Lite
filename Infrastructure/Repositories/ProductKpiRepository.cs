using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductKpiRepository(DataContext context)
    : Repository<ProductKpi>(context), IProductKpiRepository
{
    private readonly DataContext _context = context;
    public async Task<ProductKpi> GetProductKpis(Guid companyId, CancellationToken ct)
    {
        var productKpi = await _context.ProductKpis
            .SingleOrDefaultAsync(k => k.CompanyId == companyId, ct);

        if (productKpi == null)
            throw new InvalidOperationException("ProductKpi not found.");

        return productKpi;
    }

    public async Task UpdateProductKpis(Guid companyId, CancellationToken ct)
    {
        var schedulerFailed = await FailedScheduledJobsPresent(companyId, ct);
        var totalProducts = await GetProductsCount(companyId, ct);
        var belowSafetyStock = await GetProductsBelowSafetyStockCount(companyId, ct);
        var recommendedOrders = await GetProductsWithRecommendedOrdersCount(companyId, ct);
        var withoutStockAtLeadTime = await GetProductsWithoutStockAtLeadTimeCount(companyId, ct);
        var withoutForecast = await GetProductsWithoutForecastCount(companyId, ct);
        var company = await _context.Companies.SingleOrDefaultAsync(c => c.Id == companyId, ct);

        var productKpi = new ProductKpi()
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

        var productKpiExist = await _context.ProductKpis.AnyAsync(k => k.CompanyId == companyId, ct);

        if (!productKpiExist)
            await _context.AddAsync(productKpi, ct);
        else 
            _context.Update(productKpi);

        var result = await _context.SaveChangesAsync(ct);

        if (result == 0)
            throw new InvalidOperationException("Could not update ProductKpi."); 
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

    private async Task<int> GetProductsCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.CompanyId == companyId)
            .CountAsync(ct);
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

    private async Task<int> GetProductsWithoutStockAtLeadTimeCount(Guid companyId, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => 
                p.CompanyId == companyId && 
                p.StockLevel!.NetStockPosition <= 0
            )
            .CountAsync(ct);
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
