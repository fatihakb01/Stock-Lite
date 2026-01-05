using System.Data;
using Domain.Entities.Derived;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PurchaseForecastRepository
    : Repository<PurchaseForecast>, IPurchaseForecastRepository
{
    private readonly DataContext _context;

    public PurchaseForecastRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ICollection<PurchaseForecast>> GetByProductAsync(Guid productId)
    {
        DateTime today = DateTime.UtcNow;
        int diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateTime startDate = today.AddDays(-diff);
        DateTime endDate = today.AddDays(357);

        return await _context.PurchaseForecasts
            .Include(pf => pf.Product)
            .Where(pf => 
                pf.ProductId == productId &&
                pf.WeekStartDate >= startDate &&
                pf.WeekEndDate <= endDate
            )
            .OrderBy(pf => pf.WeekStartDate)
            .ToListAsync();
    }

    public async Task InsertPurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.InsertPurchaseForecasts", companyId, productIds);
    }

    public async Task UpdatePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.UpdatePurchaseForecasts", companyId, productIds);
    }

    public async Task DeletePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.DeletePurchaseForecasts", companyId, productIds);
    }

    private async Task ExecuteProcAsync(string procName, Guid companyId, IEnumerable<Guid>? productIds)
    {
        var paramCompany = new SqlParameter("@CompanyId", companyId);

        var productIdsTable = new DataTable();
        productIdsTable.Columns.Add("Id", typeof(Guid));

        if (productIds != null)
        {
            foreach (var id in productIds)
                productIdsTable.Rows.Add(id);
        }

        var paramProductIds = new SqlParameter("@ProductIds", productIdsTable)
        {
            TypeName = "dbo.GuidList",
            SqlDbType = SqlDbType.Structured
        };

        await _context.Database.ExecuteSqlAsync(
            $"EXEC {procName} {paramCompany}, {paramProductIds}");
    }
}
