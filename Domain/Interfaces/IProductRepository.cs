using Domain.Entities.Core;

namespace Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetWithStockAsync(Guid productId, Guid companyId);
    Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, Guid companyId, CancellationToken cancellationToken);

}
