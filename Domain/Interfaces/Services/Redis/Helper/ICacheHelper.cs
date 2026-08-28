namespace Domain.Interfaces.Services.Redis.Helper
{
    public interface ICacheHelper
    {
        string GetKey(Guid id, object prefix);
        string GetKey(Guid id, object prefix, object suffix);
        Task RemoveKeysByIdsAsync(List<Guid>? ids, object prefix, CancellationToken ct);
        Task RemoveKeysByPeriodsAsync(object prefix, List<Guid>? ids, CancellationToken ct = default);
    }
}