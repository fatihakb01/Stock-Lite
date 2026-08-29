using Domain.Interfaces.Services.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class NotificationService(IHubClients clients) : INotificationService
    {
        public async Task NotifyCompanyGroup(Guid companyId, string method, object? arg1, object? arg2, CancellationToken ct)
        {
            await NotifyGroup("company", companyId, method, arg1, arg2, ct);
        }

        public async Task NotifyProductGroups(List<Guid> productIds, string method, object? arg2, CancellationToken ct)
        {
            var tasks = productIds.Select(productId => NotifyProductGroup(productId, method, arg2, ct));
            await Task.WhenAll(tasks);
        }

        private async Task NotifyProductGroup(Guid productId, string method, object? arg2, CancellationToken ct)
        {
            await NotifyGroup("product", productId, method, productId, arg2, ct);
        }

        private async Task NotifyGroup(string groupPrefix, Guid groupId, string method, object? arg1, object? arg2, CancellationToken ct)
        {
            await clients.Group($"{groupPrefix}-{groupId}").SendAsync(method, arg1, arg2, ct);
        }
    }
}
