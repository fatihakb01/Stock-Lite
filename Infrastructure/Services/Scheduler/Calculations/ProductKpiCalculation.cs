using Domain.Entities.Derived;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class ProductKpiCalculation(
    IProductKpiRepository repo, 
    IBaseCacheService<ProductKpi> cacheService
) : IProductKpiCalculation
{
    private readonly IProductKpiRepository _repo = repo;
    private readonly IBaseCacheService<ProductKpi> _cacheService = cacheService;
    public async Task RunAsync(Guid companyId, CancellationToken ct)
    {
        await _repo.UpdateProductKpis(companyId, ct);
        await _cacheService.RemoveKeysAsync(companyId, null, null, ct);
    }
}
