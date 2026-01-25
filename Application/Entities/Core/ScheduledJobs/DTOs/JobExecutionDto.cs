using Domain.Enums;

namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class JobExecutionDto
{
    public required string JobTaskName { get; set; }
    public required string ScheduledJobName { get; set; }
    public JobExecutionStatus Status { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
