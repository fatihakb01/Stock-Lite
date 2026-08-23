using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class UserCacheService(ICacheService cacheService)
        : BaseCacheService<User>(cacheService), IBaseCacheService<User>
    {
        public override string GetKey(Guid id, object key)
        {
            return key switch
            {
                UserKey.User => $"user:{id}",
                UserKey.CurrentUser => $"currentuser:{id}",
                UserKey.Users => $"users:{id}",
                _ => throw new InvalidOperationException($"Unsupported user key type: {key}")
            };
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            Guid userId = ids != null ?  ids[0] : Guid.Empty;

            switch (key)
            {
                case null:
                    await RemoveUserByIdAsync(companyId, UserKey.Users, ct);
                    await RemoveUserByIdAsync(userId, UserKey.User, ct);
                    break;

                case UserKey.Users:
                    await RemoveUserByIdAsync(companyId, UserKey.Users, ct);
                    break;

                case UserKey.CurrentUser:
                    await RemoveUserByIdAsync(userId, UserKey.CurrentUser, ct);
                    break;

                case UserKey.User:
                    await RemoveUserByIdAsync(userId, UserKey.User, ct);
                    break;

                default:
                    throw new ArgumentException($"Unsupported key type: {key}", nameof(key));
            }
        }

        private async Task RemoveUserByIdAsync(Guid id, UserKey key, CancellationToken ct)
        {
            await _cacheService.RemoveAsync(GetKey(id, key), ct);
        }
    }
}
