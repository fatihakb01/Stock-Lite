using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ISalesAggregateRepository : IRepository<SalesAggregate>
{
    // Task EnsureMissingPeriodsAsync(Guid productId, Period periodType);
    Task EnsureAllMissingPeriodsAsync(Guid companyId);
    Task<IEnumerable<SalesAggregate>> GetByProductIdAsync(Guid productId, Period periodType);
    Task UpsertSalesAggregatesAsync(IEnumerable<SalesAggregate> aggregates);
    Task UpdateSalesAggregatesStoredProcAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
}
