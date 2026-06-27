using Domain.Interfaces.Services.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class SchedulerNotificationService(IHubClients clients)
        : ISchedulerNotificationService 
    {
        public async Task NotifyJobTaskUpdated(Guid companyId, Guid jobTaskId, CancellationToken ct)
        {
            await clients.Group($"company-{companyId}")
                .SendAsync("JobTaskUpdated", jobTaskId, ct);
        }

        public async Task NotifyScheduledJobUpdated(Guid companyId, Guid scheduledJobId, CancellationToken ct)
        {
            await clients.Group($"company-{companyId}")
                .SendAsync("ScheduledJobUpdated", scheduledJobId, ct);
        }
    }
}