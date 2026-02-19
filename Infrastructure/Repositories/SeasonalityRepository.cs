using System.Data;
using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SeasonalityRepository
    : Repository<Seasonality>, ISeasonalityRepository
{
    private readonly DataContext _context;
    public SeasonalityRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Seasonality>> GetSeasonalityByProductAsync(Guid productId)
    {
        DateTime endDate = DateTime.UtcNow.AddMonths(12);

        return await _context.Seasonality
            .Where(s =>
                s.ProductId == productId && 
                s.PeriodStartDate < endDate
            ).ToListAsync();
    }

    public async Task DeleteSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.DeletePastSeasonality", companyId, productIds);
    }

    public async Task InsertSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.InsertSeasonality", companyId, productIds);
    }

    public async Task UpdateSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.UpdateSeasonality", companyId, productIds);
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
