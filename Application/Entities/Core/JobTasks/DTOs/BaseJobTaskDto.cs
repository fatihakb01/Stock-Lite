using Domain.Enums;

namespace Application.Entities.Core.JobTasks.DTOs;

public class BaseJobTaskDto
{
    public required string JobTaskKey { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public DateTime ScheduledAt { get; set; }
    public Period PeriodType { get; set; }

    public int Retries { get; set; } = 0;
    public int RetryDelayMinutes { get; set; } = 10;
}
