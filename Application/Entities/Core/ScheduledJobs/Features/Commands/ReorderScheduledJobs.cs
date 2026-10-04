using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.SignalR;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class ReorderScheduledJobs
{
    public class Command : IRequest<Result<Unit>>
    {
        public required ReorderScheduledJobsDto Dto { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        ISchedulerCacheService cacheService,
        ICompanyNotificationService notificationService,
        ICurrentUserService currentUserService
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.Dto.OrderedScheduledJobIds;

            await repository.ReorderScheduledJobAsync(dto, currentUserService.CompanyId, ct);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<Unit>.Failure("Failed to save the order of scheduled jobs", 400);

            var scheduledJob = await repository.SingleOrDefaultAsync(x => 
                x.Id == dto[0] &&
                x.JobTask.CompanyId == currentUserService.CompanyId, 
                ct,
                x => x.JobTask
            );

            if (scheduledJob != null)
            {
                await cacheService.RemoveScheduledJobKeysAsync(scheduledJob, ct, false);
                await notificationService.NotifyUpdated(SchedulerKey.ScheduledJob, currentUserService.CompanyId, scheduledJob, ct);
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
