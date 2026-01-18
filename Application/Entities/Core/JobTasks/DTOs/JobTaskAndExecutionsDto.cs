using Application.Entities.Core.ScheduledJobs.DTOs;

namespace Application.Entities.Core.JobTasks.DTOs;

public class JobTaskAndExecutionsDto : BaseJobTaskDto
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
    public ICollection<ScheduledJobAndExecutionsDto> Jobs { get; set; } 
        = new List<ScheduledJobAndExecutionsDto>();
}
