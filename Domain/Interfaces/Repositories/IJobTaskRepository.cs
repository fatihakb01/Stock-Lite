using Domain.Entities.Core;

namespace Domain.Interfaces.Repositories;

public interface IJobTaskRepository : IRepository<JobTask>
{
    Task<JobTask?> GetJobAndExecutionsAsync(
        Guid jobTaskId,
        CancellationToken ct
    );
}
