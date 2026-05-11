using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

public class ConfirmedOrderAggregateRepository
    : Repository<ConfirmedOrderAggregate>, IConfirmedOrderAggregateRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public ConfirmedOrderAggregateRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
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

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
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

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
    }
}
