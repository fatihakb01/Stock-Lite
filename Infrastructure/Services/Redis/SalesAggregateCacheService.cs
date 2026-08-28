using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class SalesAggregateCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<SalesAggregate>(cacheService), IBaseCacheService<SalesAggregate>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, SalesAggregateKey.SalesAggregate, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await cacheHelper.RemoveKeysByPeriodsAsync(SalesAggregateKey.SalesAggregate, ids, ct);
        }
    }
}
