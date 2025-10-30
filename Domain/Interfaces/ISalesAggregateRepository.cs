using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ISalesAggregateRepository : IRepository<SalesAggregate>
{
    Task<IEnumerable<SalesAggregate>> GetByProductIdAsync(Guid productId, Period periodType);
    Task UpsertSalesAggregatesAsync(IEnumerable<SalesAggregate> aggregates);
}
