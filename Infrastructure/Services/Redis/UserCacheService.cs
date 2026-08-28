using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class UserCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<User>(cacheService), IBaseCacheService<User>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            Guid userId = ids?.FirstOrDefault() ?? Guid.Empty;

            switch (key)
            {
                case null:
                    await _cacheService.RemoveAsync(GetKey(companyId, UserKey.Users), ct);
                    await _cacheService.RemoveAsync(GetKey(userId, UserKey.User), ct);
                    break;

                case UserKey.Users:
                    await _cacheService.RemoveAsync(GetKey(companyId, UserKey.Users), ct);
                    break;

                case UserKey.CurrentUser:
                    await _cacheService.RemoveAsync(GetKey(userId, UserKey.CurrentUser), ct);
                    break;

                case UserKey.User:
                    await _cacheService.RemoveAsync(GetKey(userId, UserKey.User), ct);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported user key type: {key}");
            }
        }
    }
}
