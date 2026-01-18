using Domain.Enums;

namespace Application.Entities.Core.ScheduledJobs.DTOs;

public class BaseScheduledJobDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public JobType JobType { get; set; }
    public StandardCalculationType? CalculationType { get; set; }
    public ImportDataType? ImportDataType { get; set; }
    public Period? PeriodType { get; set; }

    public string? Source { get; set; }
    public string? Target { get; set; }

    public int ExecutionOrder { get; set; }  
}
