using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Redis
{
    public interface ISchedulerCacheService
    {
        Task RemoveSchedulerKeys(ScheduledJob job, CancellationToken ct);
        Task RemoveScheduledJobKeysAsync(ScheduledJob job, CancellationToken ct);
        Task RemoveScheduledJobKeyAsync(ScheduledJob job, CancellationToken ct);
        Task RemoveJobTaskKeysAsync(JobTask jobTask, CancellationToken ct);
        Task RemoveJobTaskKeyAsync(JobTask jobTask, CancellationToken ct);
    }
}
