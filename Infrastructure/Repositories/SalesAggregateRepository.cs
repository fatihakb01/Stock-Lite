using System.Data;
using Domain.Entities;
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

    public async Task EnsureAllMissingPeriodsAsync(Guid companyId)
    {
        string sql = """
            WITH Periods AS (
                SELECT 'Daily' AS PeriodType, DATEADD(DAY, -v.number, CAST(GETUTCDATE() AS DATE)) AS PeriodStartDate
                FROM master.dbo.spt_values v
                WHERE v.type = 'P' AND v.number < 30
                UNION ALL
                SELECT 'Weekly', DATEADD(WEEK, -v.number, DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETUTCDATE() AS DATE)), CAST(GETUTCDATE() AS DATE)))
                FROM master.dbo.spt_values v
                WHERE v.type = 'P' AND v.number < 13
                UNION ALL
                SELECT 'Monthly', DATEFROMPARTS(YEAR(DATEADD(MONTH, -v.number, GETUTCDATE())), MONTH(DATEADD(MONTH, -v.number, GETUTCDATE())), 1)
                FROM master.dbo.spt_values v
                WHERE v.type = 'P' AND v.number < 12
            )
            INSERT INTO SalesAggregates (Id, ProductId, PeriodType, PeriodStartDate, PeriodEndDate, TotalQuantity, TotalRevenue)
            SELECT NEWID(), p.Id, pr.PeriodType, pr.PeriodStartDate,
                CASE pr.PeriodType
                    WHEN 'Daily' THEN pr.PeriodStartDate
                    WHEN 'Weekly' THEN DATEADD(DAY, 6, pr.PeriodStartDate)
                    WHEN 'Monthly' THEN EOMONTH(pr.PeriodStartDate)
                END,
                0, 0
            FROM Products p
            CROSS JOIN Periods pr
            LEFT JOIN SalesAggregates sa
            ON sa.ProductId = p.Id
            AND sa.PeriodType = pr.PeriodType
            AND sa.PeriodStartDate = pr.PeriodStartDate
            WHERE p.CompanyId = {0}
            AND sa.ProductId IS NULL;
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, companyId);
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

    public async Task UpdateSalesAggregatesStoredProcAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        var productIdsTable = new DataTable();
        productIdsTable.Columns.Add("Id", typeof(Guid));
        if (productIds != null)
            foreach (var id in productIds)
                productIdsTable.Rows.Add(id);

        var paramCompany = new SqlParameter("@CompanyId", companyId);
        var paramProductIds = new SqlParameter("@ProductIds", productIdsTable)
        {
            TypeName = "dbo.GuidList",
            SqlDbType = SqlDbType.Structured
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.UpdateSalesAggregates @CompanyId, @ProductIds",
            paramCompany, paramProductIds);
    }

}
