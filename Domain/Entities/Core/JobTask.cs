namespace Domain.Entities.Core;

public class JobTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string JobTaskKey { get; set; }

    public required string Name { get; set; }
    public string? Description { get; set; }

    public bool AllowManualTrigger { get; set; } = true;
    public bool IsArchived { get; set; }

    // public required string CronExpression { get; set; }

    public Guid CompanyId { get; set; }
    public required Company Company { get; set; }

    public ICollection<ScheduledJob> Jobs { get; set; } = new List<ScheduledJob>();
}
