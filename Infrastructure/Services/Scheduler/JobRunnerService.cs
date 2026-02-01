using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler;

public class JobRunnerService(
    IJobExecutionRepository executionRepo,
    IJobDispatcher dispatcher
    ) : IJobRunnerService
{
    private readonly IJobExecutionRepository _executionRepo = executionRepo;
    private readonly IJobDispatcher _dispatcher = dispatcher;

    public async Task<JobRunResult> RunAsync(ScheduledJob job, CancellationToken ct)
    {
        var execution = new JobExecution
        {
            ScheduledJobId = job.Id,
            ScheduledJob = job,
            StartedAt = DateTime.UtcNow,
            Status = JobExecutionStatus.Running
        };

        await _executionRepo.AddAsync(execution, ct);
        await _executionRepo.SaveChangesAsync(ct);

        try
        {
            await _dispatcher.DispatchAsync(job, ct);
            execution.Status = JobExecutionStatus.Succeeded;
            job.Result = JobRunResult.Succeeded;
            
            return job.Result;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..4000];
            job.Result = JobRunResult.Failed;

            return job.Result;
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            _executionRepo.Update(execution);
            await _executionRepo.SaveChangesAsync(ct);
        }
    }
}
