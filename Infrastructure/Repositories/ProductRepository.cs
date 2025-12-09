using Domain.Entities.Core;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly DataContext _context;

    public ProductRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, Guid companyId, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => ids.Contains(p.Id) && p.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetWithStockAsync(Guid productId, Guid companyId)
    {
        return await _context.Products
            .Include(p => p.StockLevel)
            .FirstOrDefaultAsync(p => p.Id == productId & p.CompanyId == companyId);
    }
}
