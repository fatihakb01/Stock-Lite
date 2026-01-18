using System.Data;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConfirmedOrderAggregateRepository
    : Repository<ConfirmedOrderAggregate>, IConfirmedOrderAggregateRepository
{
    private readonly DataContext _context;

    public ConfirmedOrderAggregateRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConfirmedOrderAggregate>> GetByProductIdAsync(
        Guid productId,
        Period periodType)
    {
        DateTime today = DateTime.UtcNow;
        int diff = ((7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7) + 7;

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

        return await _context.ConfirmedOrderAggregates
            .Where(coa =>
                coa.ProductId == productId &&
                coa.PeriodType == periodType &&
                coa.PeriodStartDate >= startDate &&
                coa.PeriodEndDate <= endDate
            )
            .OrderBy(coa => coa.PeriodStartDate)
            .ToListAsync();
    }

    public async Task InsertConfirmedOrderAggregatesAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds = null,
        Period? periodType = null)
    {
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.InsertDailyConfirmedOrderAggregates",
            Period.Weekly => "dbo.InsertWeeklyConfirmedOrderAggregates",
            Period.Monthly => "dbo.InsertMonthlyConfirmedOrderAggregates",
            _ => "dbo.InsertConfirmedOrderAggregates"
        };

        await ExecuteProcAsync(procedureName, companyId, productIds);
    }

    public async Task UpdateConfirmedOrderAggregatesAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds = null,
        Period? periodType = null)
    {
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.UpdateDailyConfirmedOrderAggregates",
            Period.Weekly => "dbo.UpdateWeeklyConfirmedOrderAggregates",
            Period.Monthly => "dbo.UpdateMonthlyConfirmedOrderAggregates",
            _ => "dbo.UpdateConfirmedOrderAggregates"
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
