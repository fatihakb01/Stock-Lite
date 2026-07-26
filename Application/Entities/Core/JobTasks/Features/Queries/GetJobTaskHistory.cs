using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTaskHistory
{
    public class Query : IRequest<Result<IReadOnlyList<JobExecutionDto>>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ISchedulerCacheService cacheService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<IReadOnlyList<JobExecutionDto>>>
    {
        public async Task<Result<IReadOnlyList<JobExecutionDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(request.Id, SchedulerKey.JobTaskHistory);
            var cached = await cacheService.GetAsync<IReadOnlyList<JobExecutionDto>>(cacheKey, ct);
            if (cached != null)
                return Result<IReadOnlyList<JobExecutionDto>>.Success(cached);

            var executions = await repository
                .GetJobsAndExecutionsAsync(request.Id, currentUser.CompanyId, ct);

            if (!executions.Any())
            {
                await cacheService.SetAsync(cacheKey, executions, TimeSpan.FromMinutes(15), ct);
                return Result<IReadOnlyList<JobExecutionDto>>.Success(new List<JobExecutionDto>());
            }

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

            return Result<IReadOnlyList<JobExecutionDto>>.Success(dto);
        }
    }
}
