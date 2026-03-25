using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler;

public class JobRunnerService(
    IJobExecutionRepository executionRepo,
    IJobDispatcher dispatcher,
    IProductKpiRepository productKpiRepository
    ) : IJobRunnerService
{
    private readonly IJobExecutionRepository _executionRepo = executionRepo;
    private readonly IJobDispatcher _dispatcher = dispatcher;
    private readonly IProductKpiRepository _productKpiRepository = productKpiRepository;

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

        try
        {
            await _dispatcher.DispatchAsync(job, ct);
            execution.Status = JobExecutionStatus.Succeeded;
            job.Result = JobExecutionStatus.Succeeded;
            
            return job.Result;
        }
        catch (Exception ex)
        {
            execution.Status = JobExecutionStatus.Failed;
            execution.ErrorMessage = ex.ToString()[..^1];
            job.Result = JobExecutionStatus.Failed;

            return job.Result;
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            _executionRepo.Update(execution);
            await _executionRepo.SaveChangesAsync(ct);
            await _productKpiRepository.UpdateSchedulerStatus(job.JobTask.CompanyId, ct);
        }
    }
}
