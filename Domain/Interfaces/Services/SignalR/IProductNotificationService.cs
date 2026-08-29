namespace Domain.Interfaces.Services.SignalR
{
    public interface IProductNotificationService
    {
        Task NotifyUpdated(object prefix, List<Guid> productIds, object? o, CancellationToken ct);
    }
}
