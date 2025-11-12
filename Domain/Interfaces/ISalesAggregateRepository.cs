using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ISalesAggregateRepository : IRepository<SalesAggregate>
{
    Task InsertSalesAggregatesAsync(Guid companyId, Period? periodType = null);
    Task<IEnumerable<SalesAggregate>> GetByProductIdAsync(Guid productId, Period periodType);
    Task UpdateSalesAggregatesAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null);
}
