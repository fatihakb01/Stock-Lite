using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class EditScheduledJob
{
    public class Command : IRequest<Result<ScheduledJobDto>>
    {
        public Guid Id { get; set; }
        public required EditScheduledJobDto EditScheduledJobDto { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        IJobTaskRepository jobTaskRepo,
        ISchedulerCacheService cacheService,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Command, Result<ScheduledJobDto>>
    {
        public async Task<Result<ScheduledJobDto>> Handle(Command request, CancellationToken ct)
        {
            var scheduledJob = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id && 
                x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );

            if (scheduledJob == null)
                return Result<ScheduledJobDto>.Failure("ScheduledJob not found", 404);

            var dto = request.EditScheduledJobDto;

            var jobTask = await jobTaskRepo.SingleOrDefaultAsync(x =>
                x.JobTaskKey == dto.JobTaskKey &&
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (jobTask == null)
                return Result<ScheduledJobDto>.Failure("JobTaskKey not found", 404);

            scheduledJob.JobTaskId = jobTask.Id;
            mapper.Map(dto, scheduledJob);
            
            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<ScheduledJobDto>.Failure("Failed to update ScheduledJob", 400);

            var updatedScheduledJob = mapper.Map<ScheduledJobDto>(scheduledJob);

            await cacheService.RemoveScheduledJobKeysAsync(scheduledJob, ct);

            return Result<ScheduledJobDto>.Success(updatedScheduledJob);
        }
    }
}
