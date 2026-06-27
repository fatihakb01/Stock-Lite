using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler;

public class JobRunnerService(
    IJobExecutionRepository executionRepo,
    IJobDispatcher dispatcher,
    ISchedulerCacheService cacheService,
    ISchedulerNotificationService notifier
    ) : IJobRunnerService
{
    private readonly IJobExecutionRepository _executionRepo = executionRepo;
    private readonly IJobDispatcher _dispatcher = dispatcher;
    private readonly ISchedulerCacheService _cacheService = cacheService;
    private readonly ISchedulerNotificationService _notifier = notifier;

    public async Task<JobExecutionStatus> RunAsync(ScheduledJob job, JobTaskExecution jobTaskExecution, CancellationToken ct)
    {
        var execution = new JobExecution
        {
            ScheduledJobId = job.Id,
            ScheduledJob = job,
            JobTaskExecutionId = jobTaskExecution.Id,
            JobTaskExecution = jobTaskExecution,
            StartedAt = DateTime.UtcNow,
            Status = JobExecutionStatus.Running,
            TriggerType = jobTaskExecution.TriggerType
        };

        await _executionRepo.AddAsync(execution, ct);
        await _executionRepo.SaveChangesAsync(ct);

        //var locked = await _scheduledJobRepo.TryLockTaskAsync(job.Id);

        //if (locked)
        //{
        // potentially move try/catch block in here
        //}

        try
        {
            await _cacheService.RemoveScheduledJobKeysAsync(job, ct);
            await _notifier.NotifyScheduledJobUpdated(job.JobTask.CompanyId, job.Id, ct);
            await _dispatcher.DispatchAsync(job, ct);
            execution.Status = JobExecutionStatus.Succeeded;
            return JobExecutionStatus.Succeeded;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..^1];
            return JobExecutionStatus.Failed;
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            _executionRepo.Update(execution);
            await _executionRepo.SaveChangesAsync(ct);
        }
    }
}
