using Domain.Entities.Core;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class SchedulerCacheService(ICacheService cacheService) : ISchedulerCacheService
    {
        private readonly ICacheService _cacheService = cacheService;

        public async Task RemoveSchedulerKeys(ScheduledJob job, CancellationToken ct)
        {
            if (job.JobTask != null)
            {
                await RemoveJobTaskKeysAsync(job.JobTask, ct);
            }
            await RemoveScheduledJobKeysAsync(job, ct);
        }
        public async Task RemoveScheduledJobKeysAsync(ScheduledJob job, CancellationToken ct, bool removeHistory = true)
        {
            await RemoveScheduledJobKeyAsync(job, ct);

            if (removeHistory)
                await RemoveScheduledJobHistoryKeyAsync(job, ct);

            if (job.JobTask != null)
                await RemoveJobTaskKeysAsync(job.JobTask, ct, removeHistory);
        }

        public async Task RemoveScheduledJobKeyAsync(ScheduledJob job, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"scheduledjob:{job.Id}", ct);
        }

        public async Task RemoveJobTaskKeysAsync(JobTask? jobTask, CancellationToken ct, bool removeHistory = true)
        {
            await RemoveJobTasksKeyAsync(jobTask, ct);
            await RemoveJobTaskKeyAsync(jobTask, ct);

            if (removeHistory)
                await RemoveJobTaskHistoryKeysAsync(jobTask, ct);
        }

        public async Task RemoveJobTaskKeyAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
                await _cacheService.RemoveAsync($"jobtask:{jobTask.Id}", ct);
        }

        public async Task RemoveJobTasksKeyAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
                await _cacheService.RemoveAsync($"jobtasks:{jobTask.CompanyId}", ct);
        }

        private async Task RemoveJobTaskHistoryKeysAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
            {
                await _cacheService.RemoveAsync($"jobtask:history:{jobTask.Id}", ct);
                await _cacheService.RemoveAsync($"jobtasks:recent:history:{jobTask.CompanyId}", ct);
            }
        }

        private async Task RemoveScheduledJobHistoryKeyAsync(ScheduledJob job, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"scheduledjob:history:{job.Id}", ct);
        }
    }
}
