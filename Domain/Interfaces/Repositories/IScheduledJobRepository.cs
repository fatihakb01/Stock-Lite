using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IScheduledJobRepository : IRepository<ScheduledJob>
{
    Task AddScheduledJobAsync(ScheduledJob job, CancellationToken ct);
    Task DeleteScheduledJobAsync(Guid jobId, CancellationToken ct);
    Task ReorderScheduledJobAsync(List<Guid> OrderedScheduledJobIds, Guid companyId, CancellationToken ct);
    Task<IReadOnlyList<JobExecution>> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, Guid companyId, CancellationToken ct
    );
}
