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

        await executionRepo.AddAsync(execution, ct);
        await executionRepo.SaveChangesAsync(ct);  

        try
        {
            await dispatcher.DispatchAsync(jobTask, execution, ct);

            if (jobTask.Result == JobExecutionStatus.Failed)
                throw new Exception("JobTask failed. Please have a look at the errors inside the failed ScheduledJob.");

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
            execution.FinishedAt = DateTime.UtcNow;
            executionRepo.Update(execution);
            await executionRepo.SaveChangesAsync(ct);
        }
    }
}
