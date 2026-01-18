using Domain.Enums;
using Domain.Interfaces;
using Domain.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Calculations;

public class ConfirmedOrderAggregateCalculation(IConfirmedOrderAggregateRepository repo)
    : IConfirmedOrderAggregateCalculation
{
    private readonly IConfirmedOrderAggregateRepository _repo = repo;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        Period? periodType, 
        CancellationToken ct
    )
    {
        await _repo.InsertConfirmedOrderAggregatesAsync(companyId, productIds, periodType);
        await _repo.UpdateConfirmedOrderAggregatesAsync(companyId, productIds, periodType);
    }
}
