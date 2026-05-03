using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services.Scheduler
{
    public class SchedulerWorker(IServiceScopeFactory scopeFactory) 
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                await ExecuteOnceAsync(scope.ServiceProvider, ct);

                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }

        internal static async Task ExecuteOnceAsync(IServiceProvider provider, CancellationToken ct)
        {
            var companyRepository = provider
                .GetRequiredService<ICompanyRepository>();

            var jobTaskRepository = provider
                .GetRequiredService<IJobTaskRepository>();

            var service = provider
                .GetRequiredService<IJobTaskRunnerService>();

            var companyIds = await companyRepository.GetCompanyIds(ct);

            foreach (var companyId in companyIds)
            {
                var isBlocked = await jobTaskRepository.HasExhaustedTaskAsync(companyId, ct);

                if (isBlocked)
                    continue;

                var task = await jobTaskRepository.GetNextDueTaskAsync(companyId, DateTime.UtcNow, ct);

                if (task == null)
                    continue;

                var locked = await jobTaskRepository.TryLockTaskAsync(task.Id, DateTime.UtcNow);

                if (locked)
                {
                    JobTriggerType triggerType = task.Retries == 0
                        ? JobTriggerType.Scheduled
                        : JobTriggerType.Retry;

                    await service.RunAsync(task, triggerType, ct);
                }
            }
        }
    }
}
