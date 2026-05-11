using System.Data;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ForecastRepository : Repository<Forecast>, IForecastRepository
{
    private readonly DataContext _context;

    public ForecastRepository(DataContext context) : base(context)
    {
        _context = context;
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

        await ExecuteProcAsync(procedureName, companyId, productIds);
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

        await ExecuteProcAsync(procedureName, companyId, productIds);
    }

    private async Task ExecuteProcAsync(
        string procName, 
        Guid companyId, 
        IEnumerable<Guid>? productIds
    )
    {
        var paramCompany = new SqlParameter("@CompanyId", companyId);

        var productIdsTable = new DataTable();
        productIdsTable.Columns.Add("Id", typeof(Guid));
        if (productIds != null)
            foreach (var id in productIds)
                productIdsTable.Rows.Add(id);

        var paramProductIds = new SqlParameter("@ProductIds", productIdsTable)
        {
            TypeName = "dbo.GuidList",
            SqlDbType = SqlDbType.Structured
        };

        await _context.Database.ExecuteSqlAsync(
            $"EXEC {procName} {paramCompany}, {paramProductIds}");
    }
}
