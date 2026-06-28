using Domain.Entities.Core;

namespace Domain.Interfaces.Services.SignalR
{
    public interface ISchedulerNotificationService
    {
        Task NotifyJobTaskUpdated(Guid companyId, Guid jobTaskId, CancellationToken ct);
        Task NotifyScheduledJobUpdated(Guid companyId, Guid scheduledJobId, CancellationToken ct);
        Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct);
    }
}
