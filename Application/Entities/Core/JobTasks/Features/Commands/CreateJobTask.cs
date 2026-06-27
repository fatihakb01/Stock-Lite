using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class CreateJobTask
{
    public class Command : IRequest<Result<JobTaskDetailDto>>
    {
        public required CreateJobTaskDto CreateJobTaskDto { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ISchedulerCacheService cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<JobTaskDetailDto>>
    {
        public async Task<Result<JobTaskDetailDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CreateJobTaskDto;

            var jobTaskKeyExist = await repository.ExistsAsync(x =>
                x.JobTaskKey == dto.JobTaskKey && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (jobTaskKeyExist)
                return Result<JobTaskDetailDto>.Failure("JobTaskKey must be unique", 400);

            var jobTask = mapper.Map<JobTask>(dto);
            jobTask.CompanyId = currentUser.CompanyId;
            jobTask.AllowManualTriggerOnly = dto.ScheduledAt == null;
            
            if (dto.ScheduledAt != null)
            {
                jobTask.NextRunAt = dto.ScheduledAt;
                jobTask.Retries = 0;
                jobTask.MaxRetries = dto.MaxRetries ?? 1;
                jobTask.RetryDelayMinutes = dto.RetryDelayMinutes ?? 1;
                jobTask.PeriodType = dto.PeriodType;
            }

            await repository.AddAsync(jobTask, ct);
            
            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<JobTaskDetailDto>.Failure("Failed to create JobTask", 400);

            var createdJobTaskDto = mapper.Map<JobTaskDetailDto>(jobTask);

            await cacheService.RemoveJobTasksKeyAsync(jobTask, ct);

            return Result<JobTaskDetailDto>.Success(createdJobTaskDto);
        }
    }
}
