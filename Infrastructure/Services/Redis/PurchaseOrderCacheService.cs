using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class PurchaseOrderCacheService(ICacheService cacheService)
        : BaseCacheService<Product>(cacheService), IBaseCacheService<PurchaseOrder>
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
    }
}
