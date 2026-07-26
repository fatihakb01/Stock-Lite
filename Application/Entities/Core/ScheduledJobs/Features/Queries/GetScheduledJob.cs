using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Queries;

public class GetScheduledJob
{
    public class Query : IRequest<Result<ScheduledJobDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        ISchedulerCacheService cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ScheduledJobDto>>
    {
        public async Task<Result<ScheduledJobDto>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(request.Id, SchedulerKey.ScheduledJob);
            var cached = await cacheService.GetAsync<ScheduledJobDto>(cacheKey, ct);
            if (cached != null)
                return Result<ScheduledJobDto>.Success(cached);

            var scheduledJob = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.JobTask.CompanyId == currentUser.CompanyId,
                ct,
                x => x.JobTask
            );

            if (scheduledJob == null)
                return Result<ScheduledJobDto>.Failure("scheduledJob not found", 404);

            var scheduledJobDto = mapper.Map<ScheduledJobDto>(scheduledJob);
            scheduledJobDto.JobTaskKey = scheduledJob.JobTask.JobTaskKey;

            await cacheService.SetAsync(cacheKey, scheduledJobDto, TimeSpan.FromMinutes(5), ct);

            return Result<ScheduledJobDto>.Success(scheduledJobDto);
        }
    }
}
