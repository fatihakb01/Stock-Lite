using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public abstract class BaseCacheService<T>(ICacheService cacheService) 
        : IBaseCacheService<T> where T : class
    {
        protected readonly ICacheService _cacheService = cacheService;

        public abstract string GetKey(Guid id, object key);
        public abstract Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default);

        public async Task<T1?> GetAsync<T1>(string key, CancellationToken ct = default)
        {
            return await _cacheService.GetAsync<T1>(key, ct);
        }

        public async Task SetAsync<T1>(string key, T1 value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            await _cacheService.SetAsync(key, value, expiry, ct);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(key, ct);
        }
    }
}