namespace Application.Entities.Core.JobTasks.DTOs;

public class BaseJobTaskDto
{
    public required string JobTaskKey { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
