namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class EditScheduledJobDto : BaseScheduledJobDto
{
    public required string JobTaskKey { get; set; }
    public bool IsArchived { get; set; }
}
