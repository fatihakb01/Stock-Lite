using Application.Core;
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
        ISchedulerCacheService cacheService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var scheduledJob = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );

            if (scheduledJob == null)
                return Result<Unit>.Failure("ScheduledJob not found", 404);

            var result = await repository.DeleteScheduledJobAsync(request.Id, ct);
            if (!result)
                return Result<Unit>.Failure("Failed to delete ScheduledJob", 400);

            await cacheService.RemoveScheduledJobKeysAsync(scheduledJob, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
