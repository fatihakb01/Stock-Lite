using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IScheduledJobRepository : IRepository<ScheduledJob>
{
    Task<IReadOnlyList<JobExecution>> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, Guid companyId, CancellationToken ct
    );
    Task<Dictionary<Guid, JobExecutionStatus>> GetLatestExecutionStatusByJobTaskAsync(
        Guid jobTaskId, CancellationToken ct
    );
}
