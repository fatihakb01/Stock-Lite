using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    public class CompanyHub : Hub
    {
        public async Task JoinCompanyGroup(Guid companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(companyId));
        }

        public async Task LeaveCompanyGroup(Guid companyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(companyId));
        }

        private static string GetGroupName(Guid companyId)
        {
            return $"company-{companyId}";
        }
    }
}
