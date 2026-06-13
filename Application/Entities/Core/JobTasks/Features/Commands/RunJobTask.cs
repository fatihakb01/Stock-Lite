using Application.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
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
        ICacheService cacheService,
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

            if (jobTask.Result == JobExecutionStatus.Running)
                return Result<Unit>.Failure("JobTask already running", 400);

            await jobTaskRunnerService.RunAsync(jobTask, JobTriggerType.Manual, ct);

            await cacheService.RemoveAsync("jobtasks", ct);
            await cacheService.RemoveAsync($"jobtask:{request.Id}", ct);
            await cacheService.RemoveAsync($"jobtask:history:{request.Id}", ct);
            await cacheService.RemoveAsync($"jobtask:recent:history", ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
