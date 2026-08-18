using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class ProductNotificationService(INotificationService notificationService) : IProductNotificationService
    {
        public async Task NotifyProductsUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductUpdated", ct);
        }

        public async Task NotifyProductTransactionsUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductTransactionsUpdated", ct);
        }
    }
}
