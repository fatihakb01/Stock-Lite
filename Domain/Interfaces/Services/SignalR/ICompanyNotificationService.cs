using Domain.Entities.Core;

namespace Domain.Interfaces.Services.SignalR
{
    public interface ICompanyNotificationService
    {
        Task NotifyJobTaskUpdated(Guid companyId, Guid jobTaskId, CancellationToken ct);
        Task NotifyScheduledJobUpdated(Guid companyId, ScheduledJob job, CancellationToken ct);
        Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct);
        Task NotifyProductsUpdated(Guid companyId, CancellationToken ct);
    }
}
