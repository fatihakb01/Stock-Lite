using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ProductCacheService(ICacheService cacheService) : BaseCacheService(cacheService), IProductCacheService
    {
        private new readonly ICacheService _cacheService = cacheService;

        public string GetKey(Guid id, ProductKey productKey)
        {
            return productKey switch
            {
                ProductKey.Product => $"product:{id}",
                ProductKey.Products => $"products:{id}",
                _ => throw new InvalidOperationException($"Unsupported product key type: {productKey}")
            };
        }

        public async Task RemoveProductKeys(List<Guid> productIds, Guid companyId, CancellationToken ct)
        {
            await RemoveProductKeysByProductId(productIds, ct);
            await RemoveProductKeysByCompanyId(companyId, ct);
        }

        private async Task RemoveProductKeysByProductId(List<Guid> productIds, CancellationToken ct)
        {
            var tasks = productIds.Select(productId =>
                _cacheService.RemoveAsync(GetKey(productId, ProductKey.Product), ct)
            );

            await Task.WhenAll(tasks);
        }

        private async Task RemoveProductKeysByCompanyId(Guid companyId, CancellationToken ct)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, ProductKey.Products), ct);
        }
    }
}
