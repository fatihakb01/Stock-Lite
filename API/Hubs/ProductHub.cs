using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    public class ProductHub : Hub
    {
        public async Task JoinProductGroup(Guid productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(productId));
        }

        public async Task LeaveProductGroup(Guid productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(productId));
        }

        private static string GetGroupName(Guid productId)
        {
            return $"product-{productId}";
        }
    }
}
