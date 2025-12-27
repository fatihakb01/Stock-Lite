using Domain.Entities.Derived;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IConfirmedOrderAggregateRepository 
    : IRepository<ConfirmedOrderAggregate>
{
    Task InsertConfirmedOrderAggregatesAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds = null,
        Period? periodType = null
    );

    Task UpdateConfirmedOrderAggregatesAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds = null,
        Period? periodType = null
    );

    Task<IEnumerable<ConfirmedOrderAggregate>> GetByProductIdAsync(
        Guid productId,
        Period periodType
    );
}
