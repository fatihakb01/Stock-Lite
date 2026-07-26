using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ProductCacheService(ICacheService cacheService) : BaseCacheService(cacheService), IProductCacheService
    {
        public string GetKey(Guid id, ProductKey productKey)
        {
            return productKey switch
            {
                ProductKey.Product => $"product:{id}",
                ProductKey.Products => $"products:{id}",
                _ => throw new InvalidOperationException($"Unsupported product key type: {productKey}")
            };
        }
    }
}
