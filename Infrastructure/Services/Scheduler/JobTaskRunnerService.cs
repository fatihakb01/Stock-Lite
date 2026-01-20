using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;

namespace Infrastructure.Services.Scheduler;

public class JobTaskRunnerService(
    IJobTaskRepository jobTaskRepo,
    IScheduledJobRepository scheduledJobRepo,
    IJobRunnerService jobRunner
) : IJobTaskRunnerService
{
    public async Task RunAsync(Guid jobTaskId, CancellationToken ct)
    {
        var tasks = await jobTaskRepo.FindAsync(x =>
            x.Id == jobTaskId, 
            ct
        );

        if (!tasks.Any())
            throw new InvalidOperationException("JobTask not found.");

        var jobs = tasks.SingleOrDefault()!.Jobs
            .Where(x => !x.IsArchived)
            .OrderBy(x => x.ExecutionOrder);

        var lastExecutionStatuses =await scheduledJobRepo
            .GetLatestExecutionStatusByJobTaskAsync(jobTaskId, ct);

        foreach (var job in jobs)
        {
            if (lastExecutionStatuses.TryGetValue(job.Id, out var status) &&
                status == JobExecutionStatus.Succeeded)
            {
                continue;
            }

            var result = await jobRunner.RunAsync(job, ct);

            if (result == JobRunResult.Failed)
            {
                break;
            }
        }    
    }
}
