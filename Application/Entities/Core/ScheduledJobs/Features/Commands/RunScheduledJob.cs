using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
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
                return Result<Unit>.Failure("scheduledJob not found or archived", 404);

            var jobTaskExecution = new JobTaskExecution
            {
                JobTaskId = scheduledJob.JobTaskId,
                JobTask = scheduledJob.JobTask,
                StartedAt = DateTime.UtcNow,
                Status = JobExecutionStatus.Running,
                TriggerType = JobTriggerType.Manual
            };

            await jobRunnerService.RunAsync(scheduledJob, jobTaskExecution, ct);
            scheduledJob.Result = JobRunResult.Unknown;
            await repository.SaveChangesAsync(ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
