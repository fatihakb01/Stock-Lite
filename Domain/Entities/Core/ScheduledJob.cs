using Domain.Enums;

namespace Domain.Entities.Core;

public class ScheduledJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }
    public string? Description { get; set; }

    public JobType JobType { get; set; }
    public StandardCalculationType? CalculationType { get; set; }
    public ImportDataType? ImportDataType { get; set; }
    public Period? PeriodType { get; set; }

    public string? Source { get; set; }
    public string? Target { get; set; }

    public bool IsArchived { get; set; }

    public Guid JobTaskId { get; set; }
    public required JobTask JobTask { get; set; }

    public JobExecutionStatus Result { get; set; } = JobExecutionStatus.Pending;

    public int ExecutionOrder { get; set; }  

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }

    public ICollection<JobExecution> JobExecutions { get; set; } = new List<JobExecution>();
}
