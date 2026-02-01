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
        var task = await jobTaskRepo.GetByIdAsync(jobTaskId, ct);

        if (task == null)
            throw new InvalidOperationException("JobTask not found.");

        var jobs = await scheduledJobRepo.FindAsync(x => 
            x.JobTaskId == task.Id, 
            ct
        );

        jobs = jobs.OrderBy(x => x.ExecutionOrder);

        var lastExecutionStatuses = await jobTaskRepo
            .GetScheduledJobResultByJobTaskAsync(jobTaskId, ct);

        foreach (var job in jobs)
        {
            if (lastExecutionStatuses.TryGetValue(job.Id, out var status) &&
                status == JobRunResult.Succeeded)
            {
                continue;
            }

            var result = await jobRunner.RunAsync(job, ct);

            if (result == JobRunResult.Failed)
            {
                break;
            }
        }    

        await jobTaskRepo.ResetScheduledJobResultByJobTaskAsync(jobTaskId, ct);
    }
}
