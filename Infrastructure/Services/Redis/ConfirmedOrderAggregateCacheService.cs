using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ConfirmedOrderAggregateCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<ConfirmedOrderAggregate>(cacheService), IBaseCacheService<ConfirmedOrderAggregate>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, ConfirmedOrderAggregateKey.ConfirmedOrderAggregate, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await cacheHelper.RemoveKeysByPeriodsAsync(ConfirmedOrderAggregateKey.ConfirmedOrderAggregate, ids, ct);
        }
    }
}
