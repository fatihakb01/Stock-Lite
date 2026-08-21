using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class ProductNotificationService(INotificationService notificationService) : IProductNotificationService
    {
        public async Task NotifyProductUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductUpdated", ct);
        }

        public async Task NotifyProductTransactionsUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductTransactionsUpdated", ct);
        }

        public async Task NotifyProductPurchaseOrdersUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductPurchaseOrdersUpdated", ct);
        }

        public async Task NotifyProductConfirmedOrdersUpdated(List<Guid> productIds, CancellationToken ct)
        {
            await notificationService.NotifyProductGroups(productIds, "ProductConfirmedOrdersUpdated", ct);
        }
    }
}
