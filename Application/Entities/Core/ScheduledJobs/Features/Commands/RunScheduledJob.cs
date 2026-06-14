using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class RunScheduledJob
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        ICacheService cacheService,
        IJobRunnerService jobRunnerService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var scheduledJob = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id && 
                x.JobTask.CompanyId == currentUser.CompanyId &&
                !x.IsArchived,
                ct,
                x => x.JobTask
            );

            if (scheduledJob == null)
                return Result<Unit>.Failure("ScheduledJob not found or archived", 404);

            if (scheduledJob.Result == JobExecutionStatus.Running)
                return Result<Unit>.Failure("ScheduledJob already running", 400);

            var jobTaskExecution = new JobTaskExecution
            {
                JobTaskId = scheduledJob.JobTaskId,
                JobTask = scheduledJob.JobTask,
                StartedAt = DateTime.UtcNow,
                Status = JobExecutionStatus.Running,
                TriggerType = JobTriggerType.Manual
            };

            await jobRunnerService.RunAsync(scheduledJob, jobTaskExecution, ct);
            scheduledJob.Result = JobExecutionStatus.Pending;
            await repository.SaveChangesAsync(ct);

            await cacheService.RemoveAsync($"scheduledjob:{request.Id}", ct);
            await cacheService.RemoveAsync($"scheduledjob:history:{request.Id}", ct);
            await cacheService.RemoveAsync($"jobtasks:{currentUser.CompanyId}", ct);
            await cacheService.RemoveAsync($"jobtask:{scheduledJob.JobTask.Id}", ct);
            await cacheService.RemoveAsync($"jobtask:history:{scheduledJob.JobTask.Id}", ct);
            await cacheService.RemoveAsync($"jobtasks:recent:history:{currentUser.CompanyId}", ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
