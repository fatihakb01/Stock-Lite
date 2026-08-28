using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ForecastCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<Forecast>(cacheService), IBaseCacheService<Forecast>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, ForecastKey.Forecast, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await cacheHelper.RemoveKeysByPeriodsAsync(ForecastKey.Forecast, ids, ct);
        }
    }
}
