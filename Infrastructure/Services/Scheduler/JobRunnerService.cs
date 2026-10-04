using Domain.Entities.Core;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler;

public class JobRunnerService(
    IScheduledJobRepository scheduledJobRepo,
    IJobTaskRepository jobTaskRepo,
    IProductKpiRepository productKpiRepository,
    IJobExecutionRepository executionRepo,
    IJobDispatcher dispatcher,
    ISchedulerCacheService cacheService,
    ICompanyNotificationService notifier
    ) : IJobRunnerService
{
    public async Task<JobExecutionStatus> RunAsync(ScheduledJob job, JobTaskExecution jobTaskExecution, CancellationToken ct)
    {
        JobExecutionStatus result = JobExecutionStatus.Running;
        await RefetchJobAndTaskResults(job, result, ct);
        
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
        
        await executionRepo.AddAsync(execution, ct);
        await executionRepo.SaveChangesAsync(ct);
                
        try
        {
            await dispatcher.DispatchAsync(job, ct);
            execution.Status = JobExecutionStatus.Succeeded;
            result = JobExecutionStatus.Succeeded;
            return result;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..^1];
            result = JobExecutionStatus.Failed;
            return result;
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            executionRepo.Update(execution);
            await executionRepo.SaveChangesAsync(ct);
            await RefetchJobAndTaskResults(job, result, ct);
        }
    }

    private async Task RefetchJobAndTaskResults(ScheduledJob job, JobExecutionStatus result, CancellationToken ct)
    {
        Guid companyId = job.JobTask.CompanyId;
        job.Result = result;
        job.JobTask.Result = result;
        await scheduledJobRepo.SaveChangesAsync(ct);
        await jobTaskRepo.SaveChangesAsync(ct);
        await productKpiRepository.UpdateSchedulerStatus(companyId, ct);
        await cacheService.RemoveScheduledJobKeysAsync(job, ct);
        await notifier.NotifyUpdated(SchedulerKey.ScheduledJob, companyId, job, ct);
    }
}
