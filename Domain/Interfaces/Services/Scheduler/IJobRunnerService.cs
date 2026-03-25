using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler;

public interface IJobRunnerService
{
    Task<JobExecutionStatus> RunAsync(ScheduledJob job, JobTaskExecution jobTaskExecution, CancellationToken ct);
}
