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
    Task<Dictionary<Guid, JobRunResult>> GetScheduledJobResultByJobTaskAsync(
        Guid jobTaskId, CancellationToken ct
    );
    Task ResetScheduledJobResultByJobTaskAsync(Guid jobTaskId, CancellationToken ct);
}
