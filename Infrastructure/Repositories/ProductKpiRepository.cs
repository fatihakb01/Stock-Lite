using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
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

    public async Task<JobExecutionStatus> GetSchedulerStatus(Guid companyId, CancellationToken ct)
    {
        var schedulerStatus = await _context.Database
            .SqlQueryRaw<int>(
            @"
            SELECT TOP 1 j.Result AS Value
            FROM JobTasks j
            WHERE j.CompanyId = @companyId
            ORDER BY
                CASE j.Result
                    WHEN 3 THEN 1  -- failed
                    WHEN 1 THEN 2  -- running
					WHEN 2 THEN 3  -- succeeded
                    WHEN 0 THEN 4  -- pending
                    ELSE 5
                END
            ",
                new SqlParameter("@companyId", companyId)
            ).SingleOrDefaultAsync(ct);

        return (JobExecutionStatus) schedulerStatus;
    }

    public async Task UpdateProductKpis(Guid companyId, CancellationToken ct)
    {
        var productKpi = await _context.ProductKpis
            .SingleOrDefaultAsync(k => k.CompanyId == companyId, ct);

        if (productKpi == null)
        {
            var company = await _context.Companies
                .SingleOrDefaultAsync(c => c.Id == companyId, ct);

            productKpi = new ProductKpi
            {
                CompanyId = companyId,
                Company = company!
            };

            await _context.AddAsync(productKpi, ct);
        }

        var schedulerStatus = await GetSchedulerStatus(companyId, ct);
        var totalProducts = await GetProductsCount(companyId, ct);
        var belowSafetyStock = await GetProductsBelowSafetyStockCount(companyId, ct);
        var recommendedOrders = await GetProductsWithRecommendedOrdersCount(companyId, ct);
        var withoutStockAtLeadTime = await GetProductsWithoutStockAtLeadTimeCount(companyId, ct);
        var withoutForecast = await GetProductsWithoutForecastCount(companyId, ct);

        Boolean isChanged =
            productKpi.SchedulerStatus != schedulerStatus ||
            productKpi.TotalProductCount != totalProducts ||
            productKpi.BelowSafetyStockCount != belowSafetyStock ||
            productKpi.RecommendedOrdersCount != recommendedOrders ||
            productKpi.WithoutStockAtLeadTimeCount != withoutStockAtLeadTime ||
            productKpi.WithoutForecastCount != withoutForecast;

        if (!isChanged) return;

        productKpi.SchedulerStatus = schedulerStatus;
        productKpi.TotalProductCount = totalProducts;
        productKpi.BelowSafetyStockCount = belowSafetyStock;
        productKpi.RecommendedOrdersCount = recommendedOrders;
        productKpi.WithoutStockAtLeadTimeCount = withoutStockAtLeadTime;
        productKpi.WithoutForecastCount = withoutForecast;

        var result = await _context.SaveChangesAsync(ct);

        if (result == 0)
            throw new InvalidOperationException("Could not update ProductKpi.");
    }

    public async Task UpdateSchedulerStatus(Guid companyId, CancellationToken ct)
    {
        var schedulerStatus = await GetSchedulerStatus(companyId, ct);

        var productKpi = await _context.ProductKpis
            .SingleOrDefaultAsync(k => k.CompanyId == companyId, ct);

        if (productKpi == null)
        {
            var company = await _context.Companies
                .SingleOrDefaultAsync(c => c.Id == companyId, ct);

            productKpi = new ProductKpi
            {
                SchedulerStatus = schedulerStatus,
                CompanyId = companyId,
                Company = company!
            };

            await _context.AddAsync(productKpi, ct);
        }
        else
        {
            if (productKpi.SchedulerStatus == schedulerStatus) return;

            productKpi.SchedulerStatus = schedulerStatus;
        }

        var result = await _context.SaveChangesAsync(ct);

        if (result == 0)
            throw new InvalidOperationException("Could not update ProductKpi.");
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
