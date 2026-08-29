using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.Scheduler.Calculations;

public class StockLevelCalculation(
    IStockLevelRepository repo,
    IProductRepository productRepo,
    IBaseCacheService<Product> cacheService,
    IProductNotificationService productNotification
) : IStockLevelCalculation
{
    private readonly IStockLevelRepository _repo = repo;
    private readonly IProductRepository _productRepo = productRepo;
    private readonly IBaseCacheService<Product> _cacheService = cacheService;
    private readonly IProductNotificationService _productNotification = productNotification;

    public async Task RunAsync(
        Guid companyId,
        IEnumerable<Guid>? productIds,
        CancellationToken ct
    )
    {
        await _repo.InsertStockLevelsAsync(companyId, productIds);
        await _repo.UpdateStockLevelsAsync(companyId, productIds);

        List<Guid>? affectedProductIds = productIds?.ToList() ?? await _productRepo.GetProductIds(companyId, ct);
        await _cacheService.RemoveKeysAsync(companyId, affectedProductIds, ProductKey.Product, ct);
        await _productNotification.NotifyUpdated(ProductKey.Product, affectedProductIds, null, ct);
    }
}
