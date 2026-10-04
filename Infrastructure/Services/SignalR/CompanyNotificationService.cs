using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class CompanyNotificationService(INotificationService notificationService)
        : ICompanyNotificationService 
    {
        public async Task NotifyUpdated(object prefix, Guid companyId, object? o, CancellationToken ct = default)
        {
            switch (prefix)
            {
                case ProductKey.Products:
                    await notificationService.NotifyCompanyGroup(companyId, "ProductsUpdated", companyId, ct);
                    break;
                case ProductKpiKey.ProductKpis:
                    await notificationService.NotifyCompanyGroup(companyId, "ProductKpisUpdated", companyId, ct);
                    break;
                case ProductKpiKey.SchedulerStatus:
                    await notificationService.NotifyCompanyGroup(companyId, "SchedulerStatusUpdated", companyId, ct);
                    break;
                case SchedulerKey.JobTasks:
                    await notificationService.NotifyCompanyGroup(companyId, "JobTasksUpdated", companyId, ct);
                    break;
                case SchedulerKey.JobTask:
                    Guid jobTaskId = o != null ? (Guid)o : Guid.Empty;
                    await NotifyJobTaskUpdated(companyId, jobTaskId, ct);
                    break;
                case SchedulerKey.ScheduledJob:
                    ScheduledJob? job = o != null ? (ScheduledJob)o : null;
                    await NotifyScheduledJobUpdated(companyId, job, ct);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported key type: {prefix}");
            }
        }

        public async Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct)
        {
            var tasks = jobTasks
                .SelectMany(j => j.Jobs)
                .Select(j => NotifyScheduledJobUpdated(j.JobTask.CompanyId, j, ct));
            await Task.WhenAll(tasks);
        }

        private async Task NotifyJobTaskUpdated(Guid companyId, Guid jobTaskId, CancellationToken ct)
        {
            await notificationService.NotifyCompanyGroup(companyId, "JobTaskUpdated", jobTaskId, ct);
        }

        private async Task NotifyScheduledJobUpdated(Guid companyId, ScheduledJob? job, CancellationToken ct)
        {
            if (job == null)
                return;

            Guid jobTaskId = job.JobTask != null ? job.JobTask.Id : Guid.Empty;
            await notificationService.NotifyCompanyGroup(companyId, "ScheduledJobUpdated", job.Id, ct);
            await NotifyJobTaskUpdated(companyId, jobTaskId, ct);
        }
    }
}

