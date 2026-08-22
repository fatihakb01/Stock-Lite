using Domain.Entities.Core;
using Domain.Enums;
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

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKey.Products), ct);

            var filters = Enum.GetValues<ProductFilter>();
            var tasks = filters.Select(async filter =>
                await _cacheService.RemoveAsync(GetKey(companyId, ProductKey.Products) + ":" + filter, ct));
            await Task.WhenAll(tasks);

            List<Guid> productIds = ids ?? new List<Guid>();
            var jobs = productIds.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, ProductKey.Product), ct));
            await Task.WhenAll(jobs);
        }
    }
}
