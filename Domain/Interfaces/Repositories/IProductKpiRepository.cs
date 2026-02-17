using Domain.Entities.Derived;

namespace Domain.Interfaces.Repositories;

public interface IProductKpiRepository
{
    Task<ProductKpi> GetProductKpis(Guid companyId, CancellationToken ct);
    Task UpdateProductKpis(Guid companyId, CancellationToken ct);
}
