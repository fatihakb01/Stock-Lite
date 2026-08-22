using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class PurchaseOrderCacheService(ICacheService cacheService)
        : BaseCacheService<PurchaseOrder>(cacheService), IBaseCacheService<PurchaseOrder>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                PurchaseOrderKey.PurchaseOrder => $"purchaseorder:{id}",
                PurchaseOrderKey.PurchaseOrders => $"purchaseorders:{id}",
                _ => throw new InvalidOperationException($"Unsupported purchase order key type: {key}")
            };
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, PurchaseOrderKey.PurchaseOrders), ct);

            List<Guid> productIds = ids ?? new List<Guid>();
            var tasks = productIds.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, PurchaseOrderKey.PurchaseOrder), ct));
            await Task.WhenAll(tasks);
        }
    }
}
