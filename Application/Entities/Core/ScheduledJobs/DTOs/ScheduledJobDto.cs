namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class ScheduledJobDto : BaseScheduledJobDto
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}
