using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class ForecastCacheService(ICacheService cacheService)
        : BaseCacheService<Forecast>(cacheService), IBaseCacheService<Forecast>
    {
        public override string GetKey(Guid id, object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Period key cannot be null");

            if (key is not Period period)
                throw new ArgumentException($"Expected Period enum but got {key.GetType().Name}", nameof(key));

            if (!Enum.IsDefined(period))
                throw new ArgumentOutOfRangeException(nameof(key), $"Invalid period value: {period}");

            var periodName = period.ToString().ToLowerInvariant();
            return $"forecast:{id}:{periodName}";
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            if (ids != null || ids?.Count > 0)
            {
                switch (key)
                {
                    case null:
                        await RemoveKeysByIdAndPeriod(ids, Period.Daily, ct);
                        await RemoveKeysByIdAndPeriod(ids, Period.Weekly, ct);
                        await RemoveKeysByIdAndPeriod(ids, Period.Monthly, ct);
                        break;

                    case Period.Daily:
                        await RemoveKeysByIdAndPeriod(ids, Period.Daily, ct);
                        break;

                    case Period.Weekly:
                        await RemoveKeysByIdAndPeriod(ids, Period.Weekly, ct);
                        break;

                    case Period.Monthly:
                        await RemoveKeysByIdAndPeriod(ids, Period.Monthly, ct);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported period key type: {key}");
                }
            }
        }

        private async Task RemoveKeysByIdAndPeriod(List<Guid> ids, object key, CancellationToken ct)
        {
            var tasks = ids.Select(async productId => await _cacheService.RemoveAsync(GetKey(productId, key), ct));
            await Task.WhenAll(tasks);
        }
    }
}
