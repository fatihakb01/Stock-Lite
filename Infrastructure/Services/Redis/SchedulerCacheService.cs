using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.Redis;

namespace Infrastructure.Services.Redis
{
    public class SchedulerCacheService(ICacheService cacheService) : ISchedulerCacheService
    {
        private readonly ICacheService _cacheService = cacheService;

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
        {
            return await _cacheService.GetAsync<T>(key, ct);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            await _cacheService.SetAsync(key, value, expiry, ct);
        }

        public async Task RemoveSchedulerKeys(List<JobTask> jobTasks, CancellationToken ct)
        {
            foreach (var jobTask in jobTasks)
            {
                await RemoveJobTaskKeysAsync(jobTask, ct);

                foreach(var job in jobTask.Jobs)
                {
                    await RemoveScheduledJobKeyAsync(job, ct);
                    await RemoveScheduledJobHistoryKeyAsync(job, ct);
                }
            }
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
            string key = GetKey(job.Id, SchedulerKey.ScheduledJob);
            await _cacheService.RemoveAsync(key, ct);
        }

        public async Task RemoveJobTaskKeysAsync(JobTask? jobTask, CancellationToken ct, bool removeHistory = true)
        {
            await RemoveJobTasksKeyAsync(jobTask, ct);
            await RemoveJobTaskKeyAsync(jobTask, ct);

            if (removeHistory)
                await RemoveJobTaskHistoryKeysAsync(jobTask, ct);

            if (jobTask != null)
                await RemoveSchedulerStatusKeyAsync(jobTask.CompanyId, ct);
        }

        public async Task RemoveSchedulerStatusKeyAsync(Guid companyId, CancellationToken ct)
        {
            string key = GetKey(companyId, SchedulerKey.SchedulerStatus);
            await _cacheService.RemoveAsync(key, ct);
        }

        public async Task RemoveJobTaskKeyAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
            {
                string key = GetKey(jobTask.Id, SchedulerKey.JobTask);
                await _cacheService.RemoveAsync(key, ct);
            }
        }

        public async Task RemoveJobTasksKeyAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
            {
                string key = GetKey(jobTask.CompanyId, SchedulerKey.JobTasks);
                await _cacheService.RemoveAsync(key, ct);
            }
        }

        private async Task RemoveJobTaskHistoryKeysAsync(JobTask? jobTask, CancellationToken ct)
        {
            if (jobTask != null)
            {
                string historyKey = GetKey(jobTask.Id, SchedulerKey.JobTaskHistory);
                string recentHistoryKey = GetKey(jobTask.CompanyId, SchedulerKey.JobTaskRecentHistory);
                await _cacheService.RemoveAsync(historyKey, ct);
                await _cacheService.RemoveAsync(recentHistoryKey, ct);
            }
        }

        private async Task RemoveScheduledJobHistoryKeyAsync(ScheduledJob job, CancellationToken ct)
        {
            string historyKey = GetKey(job.Id, SchedulerKey.ScheduledJobHistory);
            await _cacheService.RemoveAsync(historyKey, ct);
        }

        public string GetKey(Guid id, SchedulerKey schedulerKey)
        {
            return schedulerKey switch
            {
                SchedulerKey.JobTask => $"jobtask:{id}",
                SchedulerKey.JobTasks => $"jobtasks:{id}",
                SchedulerKey.JobTaskHistory => $"jobtask:history:{id}",
                SchedulerKey.JobTaskRecentHistory => $"jobtasks:recent:history:{id}",
                SchedulerKey.ScheduledJob => $"scheduledjob:{id}",
                SchedulerKey.ScheduledJobHistory => $"scheduledjob:history:{id}",
                SchedulerKey.SchedulerStatus => $"schedulerstatus:{id}",
                _ => throw new InvalidOperationException($"Unsupported scheduler key type: {schedulerKey}")
            };
        }
    }
}
