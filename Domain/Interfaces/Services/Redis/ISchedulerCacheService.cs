using Domain.Entities.Core;
using Domain.Enums.Keys;

namespace Domain.Interfaces.Services.Redis
{
    public interface ISchedulerCacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
        Task RemoveSchedulerKeys(List<JobTask> jobTasks, CancellationToken ct);
        Task RemoveSchedulerStatusKeyAsync(Guid companyId, CancellationToken ct);
        Task RemoveScheduledJobKeysAsync(ScheduledJob job, CancellationToken ct, bool removeHistory = true);
        Task RemoveScheduledJobKeyAsync(ScheduledJob job, CancellationToken ct);
        Task RemoveJobTaskKeysAsync(JobTask? jobTask, CancellationToken ct, bool removeHistory = true);
        Task RemoveJobTaskKeyAsync(JobTask? jobTask, CancellationToken ct);
        Task RemoveJobTasksKeyAsync(JobTask? jobTask, CancellationToken ct);
        string GetKey(Guid id, SchedulerKey schedulerKey);
    }
}
