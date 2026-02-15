using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetFilteredProducts(ProductFilter? filter, Guid companyId, CancellationToken ct);
    Task<ProductKpi> GetProductKpis(Guid companyId, CancellationToken ct);
}
