using Application.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.SignalR;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class DeleteJobTask
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ISchedulerCacheService cacheService,
        ICompanyNotificationService notificationService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (jobTask == null)
                return Result<Unit>.Failure("JobTask not found", 404);

            repository.Remove(jobTask);

            var result = await repository.SaveChangesAsync(ct);
            
            if (!result)
                return Result<Unit>.Failure("Failed to delete JobTask", 400);

            await cacheService.RemoveJobTaskKeysAsync(jobTask, ct);
            await notificationService.NotifyUpdated(SchedulerKey.JobTask, currentUser.CompanyId, jobTask.Id, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
