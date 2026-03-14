using Domain.Entities.Core;

namespace Domain.Interfaces.Services.Scheduler
{
    public interface IJobTaskDispatcher
    {
        Task DispatchAsync(
            JobTask jobTask, 
            JobTaskExecution jobTaskExecution, 
            CancellationToken ct
        );
    }
}
