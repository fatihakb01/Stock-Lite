using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ProductKpiCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<ProductKpi>(cacheService), IBaseCacheService<ProductKpi>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKpiKey.ProductKpis), ct);
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKpiKey.SchedulerStatus), ct);
        }
    }
}
