using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IForecastRepository : IRepository<Forecast>
{
    Task InsertForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null);
    Task UpdateForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null, Period? periodType = null);
    Task DeleteForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task<IEnumerable<Forecast>> GetForecastByProductAsync(Guid productId, Period period);
}
