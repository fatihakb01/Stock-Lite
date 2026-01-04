using Domain.Entities.Derived;

namespace Domain.Interfaces;

public interface IPurchaseForecastRepository : IRepository<PurchaseForecast>
{
    Task InsertPurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task UpdatePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task DeletePurchaseForecastsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);

    Task<ICollection<PurchaseForecast>> GetByProductAsync(Guid productId);
}
