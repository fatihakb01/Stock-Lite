using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler;

public class JobTaskRunnerService(
        IJobTaskExecutionRepository executionRepo,
        IJobTaskDispatcher dispatcher
    ) : IJobTaskRunnerService
{
    private readonly IJobTaskExecutionRepository _executionRepo = executionRepo;
    private readonly IJobTaskDispatcher _dispatcher = dispatcher;

    public async Task<JobRunResult> RunAsync(JobTask jobTask, JobTriggerType triggerType, CancellationToken ct)
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
            await _dispatcher.DispatchAsync(jobTask, execution, ct);
            execution.Status = JobExecutionStatus.Succeeded;
            jobTask.Result = JobRunResult.Succeeded;

            return jobTask.Result;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..^1];
            jobTask.Result = JobRunResult.Failed;

            return jobTask.Result;
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            _executionRepo.Update(execution);
            await _executionRepo.SaveChangesAsync(ct);
        }
    }
}
