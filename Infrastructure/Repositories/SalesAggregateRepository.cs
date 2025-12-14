using System.Data;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SalesAggregateRepository
    : Repository<SalesAggregate>, ISalesAggregateRepository
{
    private readonly DataContext _context;

    public SalesAggregateRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesAggregate>> GetByProductIdAsync(Guid productId, Period periodType)
    {
        DateTime startDate = periodType switch
        {
            Period.Daily => DateTime.UtcNow.AddDays(-30),
            Period.Weekly => DateTime.UtcNow.AddMonths(-3),
            Period.Monthly => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.UtcNow.AddDays(-30)
        };

        return await _context.SalesAggregates
            .Where(sa => sa.ProductId == productId
                         && sa.PeriodType == periodType
                         && sa.PeriodStartDate >= startDate)
            .OrderBy(sa => sa.PeriodStartDate)
            .ToListAsync();
    }

    public async Task InsertSalesAggregatesAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
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
            Period.Daily => "dbo.InsertDailySalesAggregates",
            Period.Weekly => "dbo.InsertWeeklySalesAggregates",
            Period.Monthly => "dbo.InsertMonthlySalesAggregates",
            _ => "dbo.InsertSalesAggregates" 
        };

        await _context.Database.ExecuteSqlAsync($"EXEC {procedureName} {paramCompany}, {paramProductIds}");
    }

    public async Task UpdateSalesAggregatesAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
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
            Period.Daily => "dbo.UpdateDailySalesAggregates",
            Period.Weekly => "dbo.UpdateWeeklySalesAggregates",
            Period.Monthly => "dbo.UpdateMonthlySalesAggregates",
            _ => "dbo.UpdateSalesAggregates"
        };

        await _context.Database.ExecuteSqlAsync($"EXEC {procedureName} {paramCompany}, {paramProductIds}");
    }
}