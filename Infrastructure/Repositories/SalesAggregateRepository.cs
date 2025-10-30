using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
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

    // public async Task UpsertSalesAggregatesAsync(IEnumerable<SalesAggregate> aggregates)
    // {
    //     foreach (var aggregate in aggregates)
    //     {
    //         var existing = await _context.SalesAggregates.FirstOrDefaultAsync(sa =>
    //             sa.ProductId == aggregate.ProductId &&
    //             sa.PeriodType == aggregate.PeriodType &&
    //             sa.PeriodStartDate == aggregate.PeriodStartDate);

    //         if (existing != null)
    //         {
    //             // Update existing record
    //             existing.TotalQuantity = aggregate.TotalQuantity;
    //             existing.TotalRevenue = aggregate.TotalRevenue;
    //             _context.SalesAggregates.Update(existing);
    //         }
    //         else
    //         {
    //             // Insert new record
    //             await _context.SalesAggregates.AddAsync(aggregate);
    //         }
    //     }

    //     await _context.SaveChangesAsync();
    // }

    public async Task UpsertSalesAggregatesAsync(IEnumerable<SalesAggregate> newAggregates)
    {
        if (!newAggregates.Any()) return;

        // Preload existing aggregates for these products in one query
        var productIds = newAggregates.Select(a => a.ProductId).Distinct().ToList();
        var periodTypes = newAggregates.Select(a => a.PeriodType).Distinct().ToList();

        var existing = await _context.SalesAggregates
            .Where(sa => productIds.Contains(sa.ProductId) && periodTypes.Contains(sa.PeriodType))
            .ToListAsync();

        foreach (var aggregate in newAggregates)
        {
            var match = existing.FirstOrDefault(sa =>
                sa.ProductId == aggregate.ProductId &&
                sa.PeriodType == aggregate.PeriodType &&
                sa.PeriodStartDate.Date == aggregate.PeriodStartDate.Date);

            if (match != null)
            {
                match.TotalQuantity = aggregate.TotalQuantity;
                match.TotalRevenue = aggregate.TotalRevenue;
                _context.SalesAggregates.Update(match);
            }
            else
            {
                await _context.SalesAggregates.AddAsync(aggregate);
            }
        }

        await _context.SaveChangesAsync();
    }

}
