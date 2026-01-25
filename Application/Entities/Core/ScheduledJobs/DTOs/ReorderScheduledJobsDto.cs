namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class ReorderScheduledJobsDto
{
    public required List<Guid> OrderedScheduledJobIds { get; set; }
}
