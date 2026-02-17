using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class ProductKpiCalculation(IProductKpiRepository repo) : IProductKpiCalculation
{
    private readonly IProductKpiRepository _repo = repo;
    public async Task RunAsync(Guid companyId, CancellationToken ct)
    {
        await _repo.UpdateProductKpis(companyId, ct);
    }
}
