using Application.Core;
using Domain.Interfaces;
using Domain.Services.Scheduler;
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
            var scheduledJobs = await repository.FindAsync(x =>
                x.Id == request.Id && x.JobTask.CompanyId == currentUser.CompanyId,
                ct,
                x => x.JobTask
            );

            if (!scheduledJobs.Any())
                return Result<Unit>.Failure("scheduledJob not found", 404);

            var scheduledJob = scheduledJobs.SingleOrDefault()!;

            await jobRunnerService.RunAsync(scheduledJob, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
