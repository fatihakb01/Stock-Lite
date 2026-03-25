using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IJobTaskRepository : IRepository<JobTask>
{
    Task<IReadOnlyList<JobExecution>> GetJobsAndExecutionsAsync(
        Guid jobTaskId,
        Guid companyId,
        CancellationToken ct
    );
    Task<List<JobTask>> GetDueTasksAsync(
        DateTime now, 
        CancellationToken ct
    );
    Task<Dictionary<Guid, JobExecutionStatus>> GetScheduledJobResultByJobTaskAsync(
        Guid jobTaskId, 
        CancellationToken ct
    );
    Task ResetScheduledJobResultByJobTaskAsync(
        Guid jobTaskId, 
        CancellationToken ct
    );
    Task ResetJobTaskResultByJobTaskAsync(
        Guid jobTaskId, 
        CancellationToken ct
    );
    Task<bool> TryLockTaskAsync(
        Guid taskId, 
        DateTime now
    );
    Task CleanJobTask(
        Guid taskId,
        CancellationToken ct
    );
}
