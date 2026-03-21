using Domain.Enums;

namespace Domain.Entities.Core;

public class JobTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string JobTaskKey { get; set; }

    public required string Name { get; set; }
    public string? Description { get; set; }

    public bool AllowManualTriggerOnly { get; set; }
    public bool IsArchived { get; set; }
    public bool IsRunning { get; set; } = false;

    public DateTime? ScheduledAt { get; set; }
    public DateTime? LastStartedAt { get; set; }
    public DateTime? NextRunAt { get; set; }

    public int? Retries { get; set; }
    public int? MaxRetries { get; set; }
    public int? RetryDelayMinutes { get; set; }

    public Period? PeriodType { get; set; }
    public JobRunResult Result { get; set; } = JobRunResult.Unknown;

    public Guid CompanyId { get; set; }
    public required Company Company { get; set; }

    public ICollection<ScheduledJob> Jobs { get; set; } = new List<ScheduledJob>();
    public ICollection<JobTaskExecution> Executions { get; set; } = new List<JobTaskExecution>();
}
