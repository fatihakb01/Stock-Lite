using Domain.Entities.Core;
using Domain.Interfaces.Services.SignalR;

namespace Infrastructure.Services.SignalR
{
    public class CompanyNotificationService(INotificationService notificationService)
        : ICompanyNotificationService 
    {
        public async Task NotifyJobTaskUpdated(Guid companyId, Guid jobTaskId, CancellationToken ct)
        {
            await notificationService.NotifyCompanyGroup(companyId, "JobTaskUpdated", jobTaskId, null, ct);
        }

        public async Task NotifyScheduledJobUpdated(Guid companyId, ScheduledJob job, CancellationToken ct)
        {
            Guid jobTaskId = job.JobTask != null ? job.JobTask.Id : Guid.Empty;
            await notificationService.NotifyCompanyGroup(companyId, "ScheduledJobUpdated", job.Id, jobTaskId, ct);
            await NotifyJobTaskUpdated(companyId, jobTaskId, ct);
        }

        public async Task NotifySchedulerFinished(List<JobTask> jobTasks, CancellationToken ct)
        {
            foreach (JobTask jobTask in jobTasks)
            {
                await NotifyJobTaskUpdated(jobTask.CompanyId, jobTask.Id, ct);

                foreach (var job in jobTask.Jobs)
                {
                    await NotifyScheduledJobUpdated(jobTask.CompanyId, job, ct);
                }
            }
        }

        public async Task NotifyProductsUpdated(Guid companyId, CancellationToken ct)
        {
            await notificationService.NotifyCompanyGroup(companyId, "ProductsUpdated", companyId, null, ct);
        }
    }
}

