using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Redis
{
    public interface ISchedulerCacheService
    {
        Task RemoveSchedulerKeys(List<JobTask> jobTasks, CancellationToken ct);
        Task RemoveSchedulerStatusKeyAsync(Guid companyId, CancellationToken ct);
        Task RemoveScheduledJobKeysAsync(ScheduledJob job, CancellationToken ct, bool removeHistory = true);
        Task RemoveScheduledJobKeyAsync(ScheduledJob job, CancellationToken ct);
        Task RemoveJobTaskKeysAsync(JobTask? jobTask, CancellationToken ct, bool removeHistory = true);
        Task RemoveJobTaskKeyAsync(JobTask? jobTask, CancellationToken ct);
        Task RemoveJobTasksKeyAsync(JobTask? jobTask, CancellationToken ct);
    }
}
