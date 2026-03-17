using Domain.Enums;

namespace Application.Entities.Core.JobTasks.DTOs;

public class BaseJobTaskDto
{
    public required string JobTaskKey { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public DateTime ScheduledAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public Period PeriodType { get; set; }

    public int MaxRetries { get; set; }
    public int RetryDelayMinutes { get; set; }
}
