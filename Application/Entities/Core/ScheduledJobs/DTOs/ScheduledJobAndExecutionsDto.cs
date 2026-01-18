namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class ScheduledJobAndExecutionsDto : BaseScheduledJobDto
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    
    public ICollection<JobExecutionDto> JobExecutions { get; set; } 
        = new List<JobExecutionDto>();
}
