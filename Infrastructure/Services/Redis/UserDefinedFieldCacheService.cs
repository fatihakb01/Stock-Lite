using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class UserDefinedFieldCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<UserDefinedField>(cacheService), IBaseCacheService<UserDefinedField>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            List<Guid> productIds = ids ?? new List<Guid>();
            var tasks = productIds.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, UserDefinedFieldKey.UserDefinedField), ct));
            await Task.WhenAll(tasks);
        }
    }
}
