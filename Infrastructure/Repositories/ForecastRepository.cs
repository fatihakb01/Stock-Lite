using System.Data;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces;
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
        DateTime startDate = periodType switch
        {
            Period.Daily => DateTime.UtcNow.AddDays(-30),
            Period.Weekly => DateTime.UtcNow.AddMonths(-3),
            Period.Monthly => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.UtcNow.AddDays(-30)
        };

        return await _context.Forecasts
            .Where(f => f.ProductId == productId
                         && f.PeriodType == periodType
                         && f.PeriodStartDate >= startDate)
            .OrderBy(f => f.PeriodStartDate)
            .ToListAsync();
    }

    public async Task InsertForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
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

        string procedureName = periodType switch
        {
            Period.Daily => "dbo.InsertDailyForecasts",
            Period.Weekly => "dbo.InsertWeeklyForecasts",
            Period.Monthly => "dbo.InsertMonthlyForecasts",
            _ => "dbo.InsertForecasts" 
        };

        await _context.Database.ExecuteSqlAsync($"EXEC {procedureName} {paramCompany}, {paramProductIds}");
    }

    public async Task UpdateForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
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

        string procedureName = periodType switch
        {
            Period.Daily => "dbo.UpdateDailyForecasts",
            Period.Weekly => "dbo.UpdateWeeklyForecasts",
            Period.Monthly => "dbo.UpdateMonthlyForecasts",
            _ => "dbo.UpdateForecasts" 
        };

        await _context.Database.ExecuteSqlAsync($"EXEC {procedureName} {paramCompany}, {paramProductIds}");
    }

    public async Task DeleteForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
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

        await _context.Database.ExecuteSqlAsync($"EXEC dbo.DeletePastForecasts {paramCompany}, {paramProductIds}");
    }
}
