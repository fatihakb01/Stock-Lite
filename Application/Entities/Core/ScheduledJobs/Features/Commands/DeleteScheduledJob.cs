using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class DeleteScheduledJob
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        IJobTaskRepository jobTaskRepository,
        ICacheService cacheService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var exist = await repository.ExistsAsync(x =>
                x.Id == request.Id &&
                x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!exist)
                return Result<Unit>.Failure("ScheduledJob not found", 404);

            var jobTask = await jobTaskRepository.SingleOrDefaultAsync(x =>
                x.Jobs.Any(j => j.Id == request.Id) &&
                x.CompanyId == currentUser.CompanyId, 
                ct
            );

            var result = await repository.DeleteScheduledJobAsync(request.Id, ct);
            if (!result)
                return Result<Unit>.Failure("Failed to delete ScheduledJob", 400);

            await cacheService.RemoveAsync($"scheduledjob:{request.Id}", ct);
            await cacheService.RemoveAsync($"scheduledjob:history:{request.Id}", ct);

            if (jobTask != null)
            {
                await cacheService.RemoveAsync($"jobtask:{jobTask.Id}", ct);
                await cacheService.RemoveAsync($"jobtask:history:{jobTask.Id}", ct);
                await cacheService.RemoveAsync($"jobtasks:recent:history:{currentUser.CompanyId}", ct);
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
