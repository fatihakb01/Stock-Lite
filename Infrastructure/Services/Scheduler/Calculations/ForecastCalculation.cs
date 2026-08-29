using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler.Calculations;

public class ForecastCalculation(
    IForecastRepository repo,
    IProductRepository productRepo,
    IBaseCacheService <Forecast> cacheService,
    IProductNotificationService productNotification
) : IForecastCalculation
{
    private readonly IForecastRepository _repo = repo;
    private readonly IProductRepository _productRepo = productRepo;
    private readonly IBaseCacheService<Forecast> _cacheService = cacheService;
    private readonly IProductNotificationService _productNotification = productNotification;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        Period? periodType, 
        CancellationToken ct
    )
    {
        await _repo.InsertForecastsAsync(companyId, productIds, periodType);
        await _repo.UpdateForecastsAsync(companyId, productIds, periodType);

        List<Guid>? affectedProductIds = productIds?.ToList() ?? await _productRepo.GetProductIds(companyId, ct);
        await _cacheService.RemoveKeysAsync(companyId, affectedProductIds, null, ct);
        await _productNotification.NotifyUpdated(ForecastKey.Forecast, affectedProductIds, periodType, ct);
    }
}
