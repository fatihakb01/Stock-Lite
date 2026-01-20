using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class StockLevelCalculation(IStockLevelRepository repo) : IStockLevelCalculation
{
    private readonly IStockLevelRepository _repo = repo;

    public async Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        CancellationToken ct
    )
    {
        await _repo.InsertStockLevelsAsync(companyId, productIds);
        await _repo.UpdateStockLevelsAsync(companyId, productIds);
    }
}
