
using Domain.Enums;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis.Helper
{
    public class CacheHelper(ICacheService cacheService) : ICacheHelper
    {
        public string GetKey(Guid id, object prefix)
        {
            var prefixName = prefix?.ToString()?.ToLowerInvariant();
            return $"{prefixName}:{id}";
        }

        public string GetKey(Guid id, object prefix, object suffix)
        {
            var prefixName = prefix?.ToString()?.ToLowerInvariant();
            var suffixName = suffix?.ToString()?.ToLowerInvariant();
            return $"{prefixName}:{id}:{suffixName}";
        }

        public async Task RemoveKeysByPeriodsAsync(object prefix, List<Guid>? ids, CancellationToken ct = default)
        {
            if (ids == null || ids.Count == 0)
                return;

            await RemoveKeysByIdsAsync(ids, prefix, Period.Daily, ct);
            await RemoveKeysByIdsAsync(ids, prefix, Period.Weekly, ct);
            await RemoveKeysByIdsAsync(ids, prefix, Period.Monthly, ct);
        }

        public async Task RemoveKeysByIdsAsync(List<Guid>? ids, object prefix, CancellationToken ct)
        {
            if (ids == null || ids.Count == 0)
                return;

            var tasks = ids.Select(async productId => await cacheService.RemoveAsync(GetKey(productId, prefix), ct));
            await Task.WhenAll(tasks);
        }

        private async Task RemoveKeysByIdsAsync(List<Guid> ids, object prefix, object suffix, CancellationToken ct)
        {
            var tasks = ids.Select(async productId => await cacheService.RemoveAsync(GetKey(productId, prefix, suffix), ct));
            await Task.WhenAll(tasks);
        }
    }
}