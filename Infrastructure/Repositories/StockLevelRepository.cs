using System.Data;
using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StockLevelRepository : Repository<StockLevel>, IStockLevelRepository
{
    private readonly DataContext _context;

    public StockLevelRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<StockLevel?> GetByProductAsync(Guid productId)
    {
        return await _context.StockLevels
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId);
    }

    public async Task InsertStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.InsertStockLevels", companyId, productIds);
    }

    public async Task UpdateStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.UpdateStockLevels", companyId, productIds);
    }

    public async Task DeleteStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await ExecuteProcAsync("dbo.DeleteStockLevels", companyId, productIds);
    }

    private async Task ExecuteProcAsync(string procName, Guid companyId, IEnumerable<Guid>? productIds)
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
