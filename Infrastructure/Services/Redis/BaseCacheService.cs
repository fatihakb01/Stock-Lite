using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public abstract class BaseCacheService(ICacheService cacheService) : ICacheService
    {
        protected readonly ICacheService _cacheService = cacheService;

        public virtual async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            return await _cacheService.GetAsync<T>(key, ct);
        }

        public virtual async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            await _cacheService.SetAsync(key, value, expiry, ct);
        }

        public virtual async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(key, ct);
        }
    }
}