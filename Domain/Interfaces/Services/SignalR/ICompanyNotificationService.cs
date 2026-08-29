using Domain.Entities.Core;

namespace Domain.Interfaces.Services.SignalR
{
    public interface ICompanyNotificationService
    {
        Task NotifyUpdated(object prefix, Guid companyId, object? o, CancellationToken ct = default);
        Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct);
    }
}
