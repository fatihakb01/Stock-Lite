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
            SELECT TOP 1 s.Result AS Value
            FROM ScheduledJobs s
            INNER JOIN JobTasks j ON j.Id = s.JobTaskId
            WHERE j.CompanyId = @companyId
            ORDER BY
                CASE s.Result
                    WHEN 3 THEN 1  -- failed
                    WHEN 1 THEN 2  -- running
                    WHEN 0 THEN 4  -- pending
                    WHEN 2 THEN 3  -- succeeded
                    ELSE 5
                END
            ",
                new SqlParameter("@companyId", companyId)
            ).SingleOrDefaultAsync(ct);

        return (JobExecutionStatus) schedulerStatus;
    }

    public async Task UpdateProductKpis(Guid companyId, CancellationToken ct)
    {
        var schedulerStatus = await GetSchedulerStatus(companyId, ct);
        var totalProducts = await GetProductsCount(companyId, ct);
        var belowSafetyStock = await GetProductsBelowSafetyStockCount(companyId, ct);
        var recommendedOrders = await GetProductsWithRecommendedOrdersCount(companyId, ct);
        var withoutStockAtLeadTime = await GetProductsWithoutStockAtLeadTimeCount(companyId, ct);
        var withoutForecast = await GetProductsWithoutForecastCount(companyId, ct);
        var company = await _context.Companies.SingleOrDefaultAsync(c => c.Id == companyId, ct);

        var productKpi = new ProductKpi()
        {
            SchedulerStatus = schedulerStatus,
            TotalProductCount = totalProducts,
            BelowSafetyStockCount = belowSafetyStock,
            RecommendedOrdersCount = recommendedOrders,
            WithoutStockAtLeadTimeCount = withoutStockAtLeadTime,
            WithoutForecastCount = withoutForecast,
            CompanyId = companyId,
            Company = company!
        };

        var productKpiExist = await _context.ProductKpis.AnyAsync(k => k.CompanyId == companyId, ct);
        if (productKpiExist)
            _context.ProductKpis.Remove(productKpi);

        await _context.AddAsync(productKpi, ct);

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
