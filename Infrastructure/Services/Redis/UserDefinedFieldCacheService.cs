using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class UserDefinedFieldCacheService(ICacheService cacheService)
        : BaseCacheService<UserDefinedField>(cacheService), IBaseCacheService<UserDefinedField>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                UserDefinedFieldKey.UserDefinedField => $"userdefinedfield:{id}",
                _ => throw new InvalidOperationException($"Unsupported user defined field key type: {key}")
            };
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid> ids, CancellationToken ct)
        {
            var tasks = ids.Select(async productId =>
                await _cacheService.RemoveAsync(GetKey(productId, UserDefinedFieldKey.UserDefinedField), ct));

            await Task.WhenAll(tasks);
        }
    }
}
