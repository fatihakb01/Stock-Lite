using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands
{
    public class CleanJobTask
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IJobTaskRepository jobTaskRepository,
            ICacheService cacheService,
            IScheduledJobRepository scheduledJobRepository,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var jobTask = await jobTaskRepository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (jobTask == null)
                    return Result<Unit>.Failure("JobTask not found", 404);

                await jobTaskRepository.CleanJobTask(request.Id, ct);
                await scheduledJobRepository.CleanScheduledJobs(request.Id, ct);

                await cacheService.RemoveAsync($"jobtasks:{currentUser.CompanyId}", ct);
                await cacheService.RemoveAsync($"jobtask:{request.Id}", ct);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
