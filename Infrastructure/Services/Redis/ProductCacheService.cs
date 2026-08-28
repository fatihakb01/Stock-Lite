using Domain.Entities.Core;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ProductCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<Product>(cacheService), IBaseCacheService<Product>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
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
