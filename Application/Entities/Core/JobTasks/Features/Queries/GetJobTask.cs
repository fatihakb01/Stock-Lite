using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTask
{
    public class Query : IRequest<Result<JobTaskDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ICacheService cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<JobTaskDetailDto>>
    {
        public async Task<Result<JobTaskDetailDto>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = $"jobtask:{request.Id}";

            var cached = await cacheService.GetAsync<JobTaskDetailDto>(cacheKey, ct);
            if (cached != null)
                return Result<JobTaskDetailDto>.Success(cached);

            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.CompanyId == currentUser.CompanyId,
                ct,
                x => x.Jobs.OrderBy(x => x.ExecutionOrder)
            );

            if (jobTask == null)
                return Result<JobTaskDetailDto>.Failure("JobTasks not found", 404);

            var jobTaskDto = mapper.Map<JobTaskDetailDto>(jobTask);

            await cacheService.SetAsync(cacheKey, jobTaskDto, TimeSpan.FromMinutes(5), ct);

            return Result<JobTaskDetailDto>.Success(jobTaskDto);
        }
    }
}
