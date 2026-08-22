using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ConfirmedOrderCacheService(ICacheService cacheService)
        : BaseCacheService<ConfirmedOrder>(cacheService), IBaseCacheService<ConfirmedOrder>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                ConfirmedOrderKey.ConfirmedOrder => $"confirmedorder:{id}",
                ConfirmedOrderKey.ConfirmedOrders => $"confirmedorders:{id}",
                _ => throw new InvalidOperationException($"Unsupported confirmed order key type: {key}")
            };
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
