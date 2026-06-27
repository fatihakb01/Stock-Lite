using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler;

public class JobTaskRunnerService(
    IJobTaskExecutionRepository executionRepo,
    IJobTaskDispatcher dispatcher,
    IProductKpiRepository productKpiRepository,
    ISchedulerCacheService cacheService,
    ISchedulerNotificationService notifier
) : IJobTaskRunnerService
{
    private readonly IJobTaskExecutionRepository _executionRepo = executionRepo;
    private readonly IJobTaskDispatcher _dispatcher = dispatcher;
    private readonly IProductKpiRepository _productKpiRepository = productKpiRepository;
    private readonly ISchedulerCacheService _cacheService = cacheService;
    private readonly ISchedulerNotificationService _notifier = notifier;

    public async Task<JobExecutionStatus> RunAsync(JobTask jobTask, JobTriggerType triggerType, CancellationToken ct)
    {

        var execution = new JobTaskExecution
        {
            JobTaskId = jobTask.Id,
            JobTask = jobTask,
            StartedAt = DateTime.UtcNow,
            Status = JobExecutionStatus.Running,
            TriggerType = triggerType
        };

        await _executionRepo.AddAsync(execution, ct);
        await _executionRepo.SaveChangesAsync(ct);  

        try
        {
            await _cacheService.RemoveJobTaskKeysAsync(jobTask, ct, true);
            await _dispatcher.DispatchAsync(jobTask, execution, ct);

            if (jobTask.Result == JobExecutionStatus.Failed)
                throw new Exception();

            execution.Status = JobExecutionStatus.Succeeded;

            return jobTask.Result;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..^1];

            return jobTask.Result;
        }
        finally
        {
            await _cacheService.RemoveJobTaskKeysAsync(jobTask, ct, true);
            await _notifier.NotifyJobTaskUpdated(jobTask.CompanyId, jobTask.Id, ct);

            execution.FinishedAt = DateTime.UtcNow;
            _executionRepo.Update(execution);

            await _executionRepo.SaveChangesAsync(ct);
            await _productKpiRepository.UpdateSchedulerStatus(jobTask.CompanyId, ct);
        }
    }
}
