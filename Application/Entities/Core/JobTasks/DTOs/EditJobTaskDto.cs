namespace Application.Entities.Core.JobTasks.DTOs;

public class EditJobTaskDto : BaseJobTaskDto
{
    public DateTime? NextRunAt { get; set; }
    public bool IsArchived { get; set; }
}
