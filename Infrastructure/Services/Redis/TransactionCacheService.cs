using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class TransactionCacheService(ICacheService cacheService)
        : BaseCacheService<Transaction>(cacheService), IBaseCacheService<Transaction>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                TransactionKey.Transaction => $"transaction:{id}",
                TransactionKey.Transactions => $"transactions:{id}",
                _ => throw new InvalidOperationException($"Unsupported transaction key type: {key}")
            };
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid> ids, CancellationToken ct)
        {
            await _cacheService.RemoveAsync(GetKey(companyId, TransactionKey.Transactions), ct);

            var tasks = ids.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, TransactionKey.Transaction), ct));

            await Task.WhenAll(tasks);
        }
    }
}
