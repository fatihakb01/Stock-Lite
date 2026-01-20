using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IScheduledJobRepository : IRepository<ScheduledJob>
{
    Task<ScheduledJob?> GetScheduledJobAndExecutionsAsync(
        Guid scheduledJobId, CancellationToken ct
    );
    Task<Dictionary<Guid, JobExecutionStatus>> GetLatestExecutionStatusByJobTaskAsync(
        Guid jobTaskId, CancellationToken ct
    );
}
