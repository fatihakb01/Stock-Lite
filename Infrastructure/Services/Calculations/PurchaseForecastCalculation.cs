using Domain.Interfaces;
using Domain.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Calculations;

public class PurchaseForecastCalculation(IPurchaseForecastRepository repo) : IPurchaseForecastCalculation
{
    private readonly IPurchaseForecastRepository _repo = repo;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        CancellationToken ct
    )
    {
        await _repo.InsertPurchaseForecastsAsync(companyId, productIds);
        await _repo.UpdatePurchaseForecastsAsync(companyId, productIds);
    }
}
