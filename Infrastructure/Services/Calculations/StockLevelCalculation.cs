using Domain.Interfaces;
using Domain.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Calculations;

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
