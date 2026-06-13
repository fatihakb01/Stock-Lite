using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetRecentHistory
{
    public class Query : IRequest<Result<List<JobExecutionDto>>>
    {
        
    }

    public class Handler(
        IJobExecutionRepository repository,
        ICacheService cacheService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<JobExecutionDto>>>
    {
        public async Task<Result<List<JobExecutionDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = $"jobtasks:recent:history";

            var cached = await cacheService.GetAsync<List<JobExecutionDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<JobExecutionDto>>.Success(cached);

            var executions = await repository.GetRecentExecutionAsync(currentUser.CompanyId, 30, ct);
            if (!executions.Any())
                return Result<List<JobExecutionDto>>.Failure("JobExecutions not found", 404);

            var dto = executions.Select(e => new JobExecutionDto
            {
                JobTaskName = e.ScheduledJob.JobTask.Name,
                ScheduledJobName = e.ScheduledJob.Name,
                Status = e.Status,
                TriggerType = e.TriggerType,
                StartedAt = e.StartedAt,
                FinishedAt = e.FinishedAt,
                ErrorMessage = e.ErrorMessage
            }).ToList();

            await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), ct);

            return Result<List<JobExecutionDto>>.Success(dto);
        }
    }
}
