using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class TransactionCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<Transaction>(cacheService), IBaseCacheService<Transaction>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, TransactionKey.Transactions), ct);

            List<Guid> productIds = ids ?? new List<Guid>();
            var tasks = productIds.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, TransactionKey.Transaction), ct));
            await Task.WhenAll(tasks);
        }
    }
}
