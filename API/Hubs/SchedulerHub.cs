using Domain.Interfaces.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    public class SchedulerHub(ICurrentUserService currentUser) : Hub
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        public async Task JoinCompanyGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"company-{_currentUser.CompanyId}");
        }
    }
}
