using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Redis.Helper;

namespace Infrastructure.Services.Redis
{
    public class ExportProfileCacheService(ICacheService cacheService, ICacheHelper cacheHelper)
        : BaseCacheService<ExportProfile>(cacheService), IBaseCacheService<ExportProfile>
    {
        public override string GetKey(Guid id, object key)
        {
            return cacheHelper.GetKey(id, key);
        }

        public override async Task RemoveKeysAsync(Guid companyId, List<Guid>? ids, object? key, CancellationToken ct = default)
        {
            Guid exportProfileId = ids?.FirstOrDefault() ?? Guid.Empty;

            switch (key)
            {
                case null:
                    await _cacheService.RemoveAsync(GetKey(companyId, ExportProfileKey.ExportProfiles), ct);
                    await _cacheService.RemoveAsync(GetKey(exportProfileId, ExportProfileKey.ExportProfile), ct);
                    break;

                case ExportProfileKey.ExportProfiles:
                    await _cacheService.RemoveAsync(GetKey(companyId, ExportProfileKey.ExportProfiles), ct);
                    break;

                case ExportProfileKey.ExportProfile:
                    await _cacheService.RemoveAsync(GetKey(exportProfileId, ExportProfileKey.ExportProfile), ct);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported export profile key type: {key}");
            }
        }
    }
}
