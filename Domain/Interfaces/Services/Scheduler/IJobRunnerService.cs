using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler;

public interface IJobRunnerService
{
    Task<JobRunResult> RunAsync(ScheduledJob job, CancellationToken ct);
}
