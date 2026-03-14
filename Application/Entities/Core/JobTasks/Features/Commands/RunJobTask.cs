using Application.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Scheduler;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class RunJobTask
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        IJobTaskRunnerService jobTaskRunnerService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (jobTask == null)
                return Result<Unit>.Failure("JobTask not found", 404);

            await jobTaskRunnerService.RunAsync(jobTask, JobTriggerType.Manual, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
