using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler
{
    public class JobTaskDispatcher (
        IJobTaskRepository jobTaskRepo,
        IScheduledJobRepository scheduledJobRepo,
        IJobRunnerService jobRunnerService,
        ISchedulerCacheService cacheService
    ) : IJobTaskDispatcher
    {
        private readonly IJobTaskRepository _jobTaskRepo = jobTaskRepo;
        private readonly IScheduledJobRepository _scheduledJobRepo = scheduledJobRepo;
        private readonly IJobRunnerService _jobRunnerService = jobRunnerService;
        private readonly ISchedulerCacheService _cacheService = cacheService;

        public async Task DispatchAsync(JobTask jobTask, JobTaskExecution jobTaskExecution, CancellationToken ct)
        {
            jobTask.LastStartedAt = DateTime.UtcNow;

            var jobs = await _scheduledJobRepo.FindAsync(x =>
                 x.JobTaskId == jobTask.Id && 
                 !x.IsArchived && 
                 x.Result != JobExecutionStatus.Succeeded,
                 ct,
                 x => x.JobTask
            );
            jobs = jobs.OrderBy(x => x.ExecutionOrder);

            bool failed = false;

            foreach (var job in jobs)
            {
                var locked = await _scheduledJobRepo.TryLockTaskAsync(job.Id);

                if (locked)
                {
                    var result = await _jobRunnerService.RunAsync(job, jobTaskExecution, ct);

                    if (result == JobExecutionStatus.Failed)
                    {
                        failed = true;
                        job.Result = JobExecutionStatus.Failed;
                        await _scheduledJobRepo.SaveChangesAsync(ct);
                        break;
                    }
                    else 
                    {
                        job.Result = JobExecutionStatus.Succeeded;
                        await _scheduledJobRepo.SaveChangesAsync(ct);
                    }

                    await _cacheService.RemoveScheduledJobKeysAsync(job, ct);
                }
            }

            if (failed)
            {
                jobTask.Retries++;
                jobTask.Result = JobExecutionStatus.Failed;

                if (jobTask.Retries < jobTask.MaxRetries)
                {
                    int retryDelayMinutes = jobTask.RetryDelayMinutes ?? 1;
                    jobTask.NextRunAt = DateTime.UtcNow.AddMinutes(retryDelayMinutes);
                }
            }
            else
            {
                jobTask.Retries = 0;
                jobTask.Result = JobExecutionStatus.Succeeded;

                if (!jobTask.AllowManualTriggerOnly)
                {
                    jobTask.NextRunAt = CalculateNextRun(jobTask.ScheduledAt, jobTask.PeriodType);
                    jobTask.ScheduledAt = CalculateNextRun(jobTask.ScheduledAt, jobTask.PeriodType);
                }

                await _jobTaskRepo.SaveChangesAsync(ct);
                await _jobTaskRepo.ResetScheduler(jobTask.CompanyId, ct);
            }

            await _jobTaskRepo.SaveChangesAsync(ct);
        }

        internal static DateTime? CalculateNextRun(DateTime? current, Period? period)
        {
            return (current.HasValue && period.HasValue) ? period switch
            {
                Period.Daily => current.Value.AddDays(1),
                Period.Weekly => current.Value.AddDays(7),
                Period.Monthly => current.Value.AddMonths(1),
                _ => throw new InvalidOperationException()
            } : null;
        }
    }
}
