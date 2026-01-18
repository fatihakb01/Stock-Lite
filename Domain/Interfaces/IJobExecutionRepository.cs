using Domain.Entities.Core;

namespace Domain.Interfaces;

public interface IJobExecutionRepository : IRepository<JobExecution>
{
    Task<IReadOnlyList<JobExecution>> GetRecentExecutionAsync(
        Guid companyId,
        int days,
        CancellationToken ct
    );
}
