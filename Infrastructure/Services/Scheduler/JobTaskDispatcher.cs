using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler
{
    public class JobTaskDispatcher (
        IJobTaskRepository jobTaskRepo,
        IScheduledJobRepository scheduledJobRepo,
        IJobRunnerService jobRunnerService
    ) : IJobTaskDispatcher
    {
        private readonly IJobTaskRepository _jobTaskRepo = jobTaskRepo;
        private readonly IScheduledJobRepository _scheduledJobRepo = scheduledJobRepo;
        private readonly IJobRunnerService _jobRunnerService = jobRunnerService;

        public async Task DispatchAsync(JobTask jobTask, JobTaskExecution jobTaskExecution, CancellationToken ct)
        {
            jobTask.LastStartedAt = DateTime.UtcNow;
            jobTask.IsRunning = true;

            var jobs = await _scheduledJobRepo.FindAsync(x =>
                 x.JobTaskId == jobTask.Id,
                 ct
             );
            jobs = jobs.OrderBy(x => x.ExecutionOrder);

            var lastExecutionStatuses = await _jobTaskRepo
                .GetScheduledJobResultByJobTaskAsync(jobTask.Id, ct);

            bool failed = false;

            foreach (var job in jobs)
            {
                if (lastExecutionStatuses.TryGetValue(job.Id, out var status) &&
                    status == JobRunResult.Succeeded)
                {
                    continue;
                }

                var result = await _jobRunnerService.RunAsync(job, jobTaskExecution, ct);

                if (result == JobRunResult.Failed)
                {
                    jobTask.Retries++;
                    failed = true;
                    break;
                }
            }

            if (failed)
            {
                if (jobTask.Retries < jobTask.MaxRetries)
                {
                    jobTask.NextRunAt = DateTime.UtcNow.AddMinutes(jobTask.RetryDelayMinutes);
                }
            }
            else
            {
                jobTask.Retries = 0;
                jobTask.NextRunAt = CalculateNextRun(jobTask.NextRunAt ?? jobTask.ScheduledAt, jobTask.PeriodType);

                await _jobTaskRepo.ResetScheduledJobResultByJobTaskAsync(jobTask.Id, ct);
                await _jobTaskRepo.ResetJobTaskResultByJobTaskAsync(jobTask.Id, ct);
            }

            jobTask.IsRunning = false;

            _jobTaskRepo.Update(jobTask);

            await _jobTaskRepo.SaveChangesAsync(ct);
        }

        private static DateTime CalculateNextRun(DateTime current, Period period)
        {
            return period switch
            {
                Period.Daily => current.AddDays(1),
                Period.Weekly => current.AddDays(7),
                Period.Monthly => current.AddMonths(1),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
