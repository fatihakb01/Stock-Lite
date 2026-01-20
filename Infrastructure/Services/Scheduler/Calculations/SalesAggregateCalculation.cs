using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class SalesAggregateCalculation(ISalesAggregateRepository repo) : ISalesAggregateCalculation
{
    private readonly ISalesAggregateRepository _repo = repo;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        Period? periodType, 
        CancellationToken ct
    )
    {
        await _repo.InsertSalesAggregatesAsync(companyId, productIds, periodType);
        await _repo.UpdateSalesAggregatesAsync(companyId, productIds, periodType);
    }
}
