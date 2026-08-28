using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class PurchaseForecastCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<PurchaseForecast>(cacheService), IBaseCacheService<PurchaseForecast>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await cacheHelper.RemoveKeysByIdsAsync(ids, PurchaseForecastKey.PurchaseForecast, ct);
        }
    }
}
