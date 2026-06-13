using Domain.Interfaces.Services.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Infrastructure.Services.Redis
{
    public class CacheService(IDistributedCache cache) : ICacheService
    {
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var data = await cache.GetStringAsync(key, ct);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
            };

            await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, ct);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await cache.RemoveAsync(key, ct);
        }
    }
}
