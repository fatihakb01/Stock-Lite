using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetFilteredProducts(ProductFilter? filter, Guid companyId, CancellationToken ct);
    Task<List<Guid>> GetProductIds(Guid companyId, CancellationToken ct);
}
