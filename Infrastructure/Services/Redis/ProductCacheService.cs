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
            switch (key)
            {
                case null:
                    await RemoveProductsAsync(companyId, ct);
                    await cacheHelper.RemoveKeysByIdsAsync(ids, ProductKey.Product, ct);
                    break;
                case ProductKey.Product:
                    await cacheHelper.RemoveKeysByIdsAsync(ids, key, ct);
                    break;
                case ProductKey.Products:
                    await RemoveProductsAsync(companyId, ct);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported product key type: {key}");
            }
        }

        private async Task RemoveProductsAsync(Guid companyId, CancellationToken ct)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKey.Products), ct);

            var filters = Enum.GetValues<ProductFilter>();
            var tasks = filters.Select(filter =>
                _cacheService.RemoveAsync(GetKey(companyId, ProductKey.Products) + ":" + filter, ct));
            await Task.WhenAll(tasks);
        }
    }
}
