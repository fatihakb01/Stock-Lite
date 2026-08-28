using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ConfirmedOrderCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<ConfirmedOrder>(cacheService), IBaseCacheService<ConfirmedOrder>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ConfirmedOrderKey.ConfirmedOrders), ct);

            List<Guid> productIds = ids ?? new List<Guid>();
            var tasks = productIds.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, ConfirmedOrderKey.ConfirmedOrder), ct));
            await Task.WhenAll(tasks);
        }
    }
}
