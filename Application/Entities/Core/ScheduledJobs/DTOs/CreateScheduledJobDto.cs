namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class CreateScheduledJobDto : BaseScheduledJobDto
{
    public required string JobTaskKey { get; set; }
}
