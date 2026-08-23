using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ProductKpiCacheService(ICacheService cacheService)
        : BaseCacheService<ProductKpi>(cacheService), IBaseCacheService<ProductKpi>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                ProductKpiKey.ProductKpis => $"productkpis:{id}",
                ProductKpiKey.SchedulerStatus => $"productkpis-schedulerstatus:{id}",
                _ => throw new InvalidOperationException($"Unsupported product key type: {key}")
            };
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKpiKey.ProductKpis), ct);
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKpiKey.SchedulerStatus), ct);
        }
    }
}
