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
        public async Task RemoveScheduledJobKeysAsync(ScheduledJob job, CancellationToken ct)
        {
            await RemoveScheduledJobKeyAsync(job, ct);
            await RemoveScheduledJobHistoryAsync(job, ct);

            if (job.JobTask != null)
            {
                await RemoveJobTaskKeysAsync(job.JobTask, ct);
            }
        }

        public async Task RemoveScheduledJobKeyAsync(ScheduledJob job, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"scheduledjob:{job.Id}", ct);
        }

        public async Task RemoveJobTaskKeysAsync(JobTask jobTask, CancellationToken ct)
        {
            await RemoveJobTaskKeyAsync(jobTask, ct);
            await RemoveJobTaskHistoryKeysAsync(jobTask, ct);
        }

        public async Task RemoveJobTaskKeyAsync(JobTask jobTask, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"jobtasks:{jobTask.CompanyId}", ct);
        }

        private async Task RemoveJobTaskHistoryKeysAsync(JobTask jobTask, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"jobtask:history:{jobTask.Id}", ct);
            await _cacheService.RemoveAsync($"jobtasks:recent:history:{jobTask.CompanyId}", ct);
        }

        private async Task RemoveScheduledJobHistoryAsync(ScheduledJob job, CancellationToken ct)
        {
            await _cacheService.RemoveAsync($"scheduledjob:history:{job.Id}", ct);
        }
    }
}
