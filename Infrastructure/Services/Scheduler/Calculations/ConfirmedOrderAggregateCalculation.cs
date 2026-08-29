using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler.Calculations;

public class ConfirmedOrderAggregateCalculation(
    IConfirmedOrderAggregateRepository repo,
    IProductRepository productRepo,
    IBaseCacheService<ConfirmedOrderAggregate> cacheService,
    IProductNotificationService productNotification
) : IConfirmedOrderAggregateCalculation
{
    private readonly IConfirmedOrderAggregateRepository _repo = repo;
    private readonly IProductRepository _productRepo = productRepo;
    private readonly IBaseCacheService<ConfirmedOrderAggregate> _cacheService = cacheService;
    private readonly IProductNotificationService _productNotification = productNotification;

    public async Task RunAsync(
        Guid companyId, 
        IEnumerable<Guid>? productIds, 
        Period? periodType, 
        CancellationToken ct
    )
    {
        await _repo.InsertConfirmedOrderAggregatesAsync(companyId, productIds, periodType);
        await _repo.UpdateConfirmedOrderAggregatesAsync(companyId, productIds, periodType);

        List<Guid>? affectedProductIds = productIds?.ToList() ?? await _productRepo.GetProductIds(companyId, ct);
        await _cacheService.RemoveKeysAsync(companyId, affectedProductIds, null, ct);
        await _productNotification.NotifyUpdated(ConfirmedOrderAggregateKey.ConfirmedOrderAggregate, affectedProductIds, periodType, ct);
    }
}
