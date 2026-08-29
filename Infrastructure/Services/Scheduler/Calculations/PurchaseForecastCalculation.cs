using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler.Calculations;

public class PurchaseForecastCalculation(
    IPurchaseForecastRepository repo,
    IProductRepository productRepo,
    IBaseCacheService<PurchaseForecast> cacheService,
    IProductNotificationService productNotification
) : IPurchaseForecastCalculation
{
    private readonly IPurchaseForecastRepository _repo = repo;
    private readonly IProductRepository _productRepo = productRepo;
    private readonly IBaseCacheService<PurchaseForecast> _cacheService = cacheService;
    private readonly IProductNotificationService _productNotification = productNotification;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        CancellationToken ct
    )
    {
        await _repo.InsertPurchaseForecastsAsync(companyId, productIds);
        await _repo.UpdatePurchaseForecastsAsync(companyId, productIds);

        List<Guid>? affectedProductIds = productIds?.ToList() ?? await _productRepo.GetProductIds(companyId, ct);
        await _cacheService.RemoveKeysAsync(companyId, affectedProductIds, null, ct);
        await _productNotification.NotifyUpdated(PurchaseForecastKey.PurchaseForecast, affectedProductIds, null, ct);
    }
}
