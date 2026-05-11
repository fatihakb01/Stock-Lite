using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

public class SalesAggregateRepository
    : Repository<SalesAggregate>, ISalesAggregateRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public SalesAggregateRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
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
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.InsertDailySalesAggregates",
            Period.Weekly => "dbo.InsertWeeklySalesAggregates",
            Period.Monthly => "dbo.InsertMonthlySalesAggregates",
            _ => "dbo.InsertSalesAggregates" 
        };

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
    }

    public async Task UpdateSalesAggregatesAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null)
    {
        string procedureName = periodType switch
        {
            Period.Daily => "dbo.UpdateDailySalesAggregates",
            Period.Weekly => "dbo.UpdateWeeklySalesAggregates",
            Period.Monthly => "dbo.UpdateMonthlySalesAggregates",
            _ => "dbo.UpdateSalesAggregates"
        };

        await _helper.ExecuteProcAsync(procedureName, companyId, productIds);
    }
}