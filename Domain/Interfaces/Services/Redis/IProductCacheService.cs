using Domain.Enums.Keys;

namespace Domain.Interfaces.Services.Redis
{
    public interface IProductCacheService : ICacheService
    {
        string GetKey(Guid id, ProductKey productKey);
        Task RemoveProductKeys(List<Guid> productIds, Guid companyId, CancellationToken ct);
    }
}
