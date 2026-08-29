namespace Domain.Interfaces.Services.SignalR
{
    public interface INotificationService
    {
        Task NotifyCompanyGroup(Guid companyId, string method, object? arg1, object? arg2, CancellationToken ct);
        Task NotifyProductGroups(List<Guid> productIds, string method, object? arg2, CancellationToken ct);
    }
}
