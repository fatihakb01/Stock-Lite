using Domain.Enums;

namespace Domain.Entities.Core
{
    public class JobTaskExecution
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid JobTaskId { get; set; }
        public required JobTask JobTask { get; set; }

        public JobExecutionStatus Status { get; set; } = JobExecutionStatus.Pending;
        public JobTriggerType TriggerType { get; set; }

        public DateTime ScheduledAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public string? ErrorMessage { get; set; }

        public ICollection<JobExecution> JobExecutions { get; set; } = new List<JobExecution>();
    }
}
