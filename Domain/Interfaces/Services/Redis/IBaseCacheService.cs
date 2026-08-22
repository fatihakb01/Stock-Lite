namespace Domain.Interfaces.Services.Redis
{
    public interface IBaseCacheService<T> : ICacheService where T : class
    {
        string GetKey(Guid id, object key);
        Task RemoveKeysAsync(Guid companyId, List<Guid> ids, CancellationToken ct);
    }
}
