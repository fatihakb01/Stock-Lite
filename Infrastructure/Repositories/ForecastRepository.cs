using System.Data;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ForecastRepository : Repository<Forecast>, IForecastRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public ForecastRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
    }

    public async Task<IEnumerable<Forecast>> GetForecastByProductAsync(Guid productId, Period periodType)
    {
        DateTime today = DateTime.UtcNow.Date;
        int diff = ((7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7) + 7;

        DateTime startDate = periodType switch
        {
            Period.Daily => today,
            Period.Weekly => today.AddDays(-diff),
            Period.Monthly => today.AddMonths(-1),
            _ => today
        };

        DateTime endDate = periodType switch
        {
            Period.Daily => today.AddDays(30),
            Period.Weekly => today.AddDays(84),
            Period.Monthly => today.AddMonths(12),
            _ => today
        };

        return await _context.Forecasts
            .Where(f => 
                f.ProductId == productId
                && f.PeriodType == periodType
                && f.PeriodStartDate >= startDate
                && f.PeriodEndDate <= endDate
            )
            .OrderBy(f => f.PeriodStartDate)
            .ToListAsync();
    }

    public async Task InsertForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
    {
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.InsertDailyForecasts",
            Period.Weekly => "dbo.InsertWeeklyForecasts",
            Period.Monthly => "dbo.InsertMonthlyForecasts",
            _ => "dbo.InsertForecasts" 
        };

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
    }

    public async Task UpdateForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
    {
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.UpdateDailyForecasts",
            Period.Weekly => "dbo.UpdateWeeklyForecasts",
            Period.Monthly => "dbo.UpdateMonthlyForecasts",
            _ => "dbo.UpdateForecasts" 
        };

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
    }

    public async Task DeleteForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period period = Period.Monthly)
    {
        await _helper.ExecuteProcAsync("dbo.DeletePastForecasts", companyId, productIds, period);
    }
}
