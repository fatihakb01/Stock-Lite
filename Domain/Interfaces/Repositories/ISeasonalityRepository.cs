using Domain.Entities.Derived;

namespace Domain.Interfaces.Repositories;

public interface ISeasonalityRepository : IRepository<Seasonality>
{
    Task InsertSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task UpdateSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task DeleteSeasonalityAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task<IEnumerable<Seasonality>> GetSeasonalityByProductAsync(Guid productId);
}
