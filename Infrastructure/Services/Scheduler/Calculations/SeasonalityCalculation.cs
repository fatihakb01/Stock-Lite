using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler.Calculations;

namespace Infrastructure.Services.Scheduler.Calculations;

public class SeasonalityCalculation(ISeasonalityRepository repo) : ISeasonalityCalculation
{
    private readonly ISeasonalityRepository _repo = repo;

    public async Task RunAsync(Guid companyId, IEnumerable<Guid>? productIds, CancellationToken ct)
    {
        await _repo.DeleteSeasonalityAsync(companyId, productIds);
        await _repo.InsertSeasonalityAsync(companyId, productIds);
        await _repo.UpdateSeasonalityAsync(companyId, productIds);
    }
}
