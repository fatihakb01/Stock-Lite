namespace Domain.Interfaces.Services.SignalR
{
    public interface IProductNotificationService
    {
        Task NotifyProductsUpdated(List<Guid> productIds, CancellationToken ct);
        Task NotifyProductTransactionsUpdated(List<Guid> productIds, CancellationToken ct);
        Task NotifyProductPurchaseOrdersUpdated(List<Guid> productIds, CancellationToken ct);
        Task NotifyProductConfirmedOrdersUpdated(List<Guid> productIds, CancellationToken ct);
    }
}
