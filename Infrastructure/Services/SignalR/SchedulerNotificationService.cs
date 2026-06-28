using Domain.Entities.Core;
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

        public async Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct)
        {
            foreach (JobTask jobTask in jobTasks)
            {
                await NotifyJobTaskUpdated(jobTask.CompanyId, jobTask.Id, ct);

                foreach (var job in jobTask.Jobs)
                {
                    await NotifyScheduledJobUpdated(jobTask.CompanyId, job.Id, ct);
                }
            }
        }
    }
}

