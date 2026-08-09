using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ProductCacheService(ICacheService cacheService) 
        : BaseCacheService<Product>(cacheService), IBaseCacheService<Product>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                ProductKey.Product => $"product:{id}",
                ProductKey.Products => $"products:{id}",
                _ => throw new InvalidOperationException($"Unsupported product key type: {key}")
            };
        }
    }
}
