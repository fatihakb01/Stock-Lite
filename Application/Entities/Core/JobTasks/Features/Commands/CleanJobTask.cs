using Application.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.SignalR;
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
            ISchedulerCacheService cacheService,
            ICompanyNotificationService notificationService,
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
                await cacheService.RemoveJobTaskKeysAsync(jobTask, ct, false);
                await notificationService.NotifyUpdated(SchedulerKey.JobTask, currentUser.CompanyId, jobTask.Id, ct);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
