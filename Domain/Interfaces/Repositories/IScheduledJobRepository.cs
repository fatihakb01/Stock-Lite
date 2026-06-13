using Domain.Entities.Core;

namespace Domain.Interfaces.Repositories;

public interface IScheduledJobRepository : IRepository<ScheduledJob>
{
    Task AddScheduledJobAsync(
        ScheduledJob job, 
        CancellationToken ct
    );
    Task<bool> DeleteScheduledJobAsync(
        Guid jobId, 
        CancellationToken ct
    );
    Task ReorderScheduledJobAsync(
        List<Guid> OrderedScheduledJobIds, 
        Guid companyId, 
        CancellationToken ct
    );
    Task<IReadOnlyList<JobExecution>> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, 
        Guid companyId, 
        CancellationToken ct
    );
    Task CleanScheduledJobs(
        Guid taskId,
        CancellationToken ct
    );
    Task CleanScheduledJob(
        Guid jobId,
        CancellationToken ct
    );
    Task<bool> TryLockTaskAsync(Guid jobId);
}
