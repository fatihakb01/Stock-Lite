namespace Application.Entities.Core.JobTasks.DTOs
{
    public class JobTaskDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
