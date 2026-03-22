using Application.Entities.Core.ScheduledJobs.DTOs;

namespace Application.Entities.Core.JobTasks.DTOs;

public class JobTaskDetailDto : BaseJobTaskDto
{
    public Guid Id { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool IsArchived { get; set; }
    public bool AllowManualTriggerOnly { get; set; }
    public ICollection<ScheduledJobDto> Jobs { get; set; } = new List<ScheduledJobDto>();
}
