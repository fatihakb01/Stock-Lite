namespace Domain.Services.Scheduler;

public interface IJobTaskRunnerService
{
    Task RunAsync(Guid jobTaskId, CancellationToken ct);
}
