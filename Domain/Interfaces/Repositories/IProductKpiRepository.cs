using Domain.Entities.Derived;
using Domain.Enums;

namespace Domain.Interfaces.Repositories;

public interface IProductKpiRepository
{
    Task<ProductKpi> GetProductKpis(Guid companyId, CancellationToken ct);
    Task<JobExecutionStatus> GetSchedulerStatus(Guid companyId, CancellationToken ct);
    Task UpdateProductKpis(Guid companyId, CancellationToken ct);
    Task UpdateSchedulerStatus(Guid companyId, CancellationToken ct);
}
