using Domain.Entities.Core;

namespace Domain.Services.Scheduler;

public interface IJobDispatcher
{
    Task DispatchAsync(ScheduledJob job, CancellationToken ct);
}
