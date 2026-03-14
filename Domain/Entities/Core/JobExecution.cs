using Domain.Enums;

namespace Domain.Entities.Core;

public class JobExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ScheduledJobId { get; set; }
    public required ScheduledJob ScheduledJob { get; set; }

    public Guid JobTaskExecutionId { get; set; }
    public required JobTaskExecution JobTaskExecution { get; set; }

    public JobExecutionStatus Status { get; set; } = JobExecutionStatus.Pending;

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
