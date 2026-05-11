using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

public class PurchaseForecastRepository
    : Repository<PurchaseForecast>, IPurchaseForecastRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public PurchaseForecastRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
    }

    public async Task<ICollection<PurchaseForecast>> GetByProductAsync(Guid productId)
    {
        DateTime today = DateTime.UtcNow.Date;
        int diff = ((7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7) + 7;

        DateTime startDate = today.AddDays(-diff);
        DateTime endDate = today.AddDays(357);

        return await _context.PurchaseForecasts
            .Include(pf => pf.Product)
            .Where(pf => 
                pf.ProductId == productId &&
                pf.WeekStartDate >= startDate &&
                pf.WeekEndDate <= endDate
            )
            .OrderBy(pf => pf.WeekStartDate)
            .ToListAsync();
    }

    public async Task InsertPurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.InsertPurchaseForecasts", companyId, productIds);
    }

    public async Task UpdatePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.UpdatePurchaseForecasts", companyId, productIds);
    }

    public async Task DeletePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.DeletePurchaseForecasts", companyId, productIds);
    }
}
