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

    public async Task EnsureMissingPeriodsAsync(Guid productId, Period periodType)
    {
        DateTime now = DateTime.UtcNow.Date;

        // Determine the lookback range
        int count = periodType switch
        {
            Period.Daily => 30,
            Period.Weekly => 13,
            Period.Monthly => 12,
            _ => 30
        };

        // Find Product based on productId
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {productId} not found.");

        // Build list of expected period start dates
        List<DateTime> expectedStartDates = new();
        for (int i = count - 1; i >= 0; i--)
        {
            DateTime start = periodType switch
            {
                Period.Daily => now.AddDays(-i),
                Period.Weekly => now.AddDays(-7 * i),
                Period.Monthly => now.AddMonths(-i),
                _ => now.AddDays(-i)
            };

            // Normalize start to beginning of the period
            if (periodType == Period.Weekly)
            {
                int diff = (7 + (start.DayOfWeek - DayOfWeek.Monday)) % 7;
                start = start.AddDays(-diff);
            }
            else if (periodType == Period.Monthly)
            {
                start = new DateTime(start.Year, start.Month, 1);
            }

            expectedStartDates.Add(start);
        }        

        // Load existing start dates
        var existingDates = await _context.SalesAggregates
            .Where(sa => sa.ProductId == productId && sa.PeriodType == periodType)
            .Select(sa => sa.PeriodStartDate.Date)
            .ToListAsync();

        // Create missing ones
        var missing = expectedStartDates
            .Where(d => !existingDates.Contains(d))
            .Select(d => new SalesAggregate
            {
                ProductId = productId,
                Product = product,
                PeriodType = periodType,
                PeriodStartDate = d,
                PeriodEndDate = periodType switch
                {
                    Period.Daily => d,
                    Period.Weekly => d.AddDays(6),
                    Period.Monthly => new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month)),
                    _ => d
                },
                TotalQuantity = 0,
                TotalRevenue = 0
            })
            .ToList();

        if (missing.Any())
        {
            await _context.SalesAggregates.AddRangeAsync(missing);
            await _context.SaveChangesAsync();
        }
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
