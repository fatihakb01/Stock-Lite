using Application.Entities.Core.ScheduledJobs.DTOs;

namespace Application.Entities.Core.JobTasks.DTOs;

public class JobTaskDto : BaseJobTaskDto
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
    public ICollection<ScheduledJobDto> Jobs { get; set; } = new List<ScheduledJobDto>();
}
