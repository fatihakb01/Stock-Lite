using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class ForecastCalculation(IForecastRepository repo) : IForecastCalculation
{
    private readonly IForecastRepository _repo = repo;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        Period? periodType, 
        CancellationToken ct
    )
    {
        await _repo.InsertForecastsAsync(companyId, productIds, periodType);
        await _repo.UpdateForecastsAsync(companyId, productIds, periodType);
    }
}
