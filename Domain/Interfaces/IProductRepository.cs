using Domain.Entities.Core;

namespace Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    public Task<Product?> GetWithStockAsync(Guid productId, Guid companyId);
}
