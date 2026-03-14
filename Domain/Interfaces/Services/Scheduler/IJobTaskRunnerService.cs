using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Services.Scheduler;

public interface IJobTaskRunnerService
{
    Task<JobRunResult> RunAsync(
        JobTask jobTask, 
        JobTriggerType triggerType, 
        CancellationToken ct
    );
}
