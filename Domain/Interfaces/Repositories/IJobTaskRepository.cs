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
    Task<JobTask?> GetNextDueTaskAsync(
        Guid companyId,
        DateTime now,
        CancellationToken ct
    );
    Task<bool> HasExhaustedTaskAsync(
        Guid companyId,
        CancellationToken ct
    );
    Task CleanJobTask(
        Guid jobTaskId,
        CancellationToken ct
    );
    Task ResetScheduler(
        Guid companyId,
        CancellationToken ct
    );
}
