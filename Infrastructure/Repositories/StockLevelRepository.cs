using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Infrastructure.Repositories.Helper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StockLevelRepository : Repository<StockLevel>, IStockLevelRepository
{
    private readonly DataContext _context;
    private readonly CallProcedureHelper _helper;

    public StockLevelRepository(DataContext context) : base(context)
    {
        _context = context;
        _helper = new CallProcedureHelper(context);
    }

    public async Task<StockLevel?> GetByProductAsync(Guid productId)
    {
        return await _context.StockLevels
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId);
    }

    public async Task InsertStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.InsertStockLevels", companyId, productIds);
    }

    public async Task UpdateStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.UpdateStockLevels", companyId, productIds);
    }

    public async Task DeleteStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null)
    {
        await _helper.ExecuteProcAsync("dbo.DeleteStockLevels", companyId, productIds);
    }
}
