using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Scheduler;

public interface IJobDispatcher
{
    Task DispatchAsync(ScheduledJob job, CancellationToken ct);
}
