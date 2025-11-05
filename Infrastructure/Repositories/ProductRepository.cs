using Domain.Entities;
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

    public async Task<Product?> GetWithStockAsync(Guid productId, Guid companyId)
    {
        return await _context.Products
            .Include(p => p.StockLevel)
            .FirstOrDefaultAsync(p => p.Id == productId & p.CompanyId == companyId);
    }
}
