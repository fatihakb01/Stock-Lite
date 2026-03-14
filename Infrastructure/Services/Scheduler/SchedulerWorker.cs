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

                var repository = scope.ServiceProvider
                    .GetRequiredService<IJobTaskRepository>();

                var service = scope.ServiceProvider
                    .GetRequiredService<IJobTaskRunnerService>();

                var dueTasks = await repository.GetDueTasksAsync(DateTime.UtcNow, ct);

                foreach (var task in dueTasks)
                {
                    var locked = await repository.TryLockTaskAsync(task.Id, DateTime.UtcNow);

                    if (!locked) continue;

                    JobTriggerType triggerType = task.Retries == 0 ? JobTriggerType.Scheduled : JobTriggerType.Retry;

                    await service.RunAsync(task, triggerType, ct);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }
    }
}
