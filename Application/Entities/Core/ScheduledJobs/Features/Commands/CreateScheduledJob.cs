using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class CreateScheduledJob
{
    public class Command : IRequest<Result<ScheduledJobDto>>
    {
        public required CreateScheduledJobDto CreateScheduledJobDto { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        IJobTaskRepository jobTaskRepository,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Command, Result<ScheduledJobDto>>
    {
        public async Task<Result<ScheduledJobDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CreateScheduledJobDto;

            var scheduledJob = mapper.Map<ScheduledJob>(dto);

            var jobTask = await jobTaskRepository.SingleOrDefaultAsync(x =>
                x.JobTaskKey == dto.JobTaskKey && 
                currentUser.CompanyId == x.CompanyId,
                ct
            );

            if (jobTask == null)
                return Result<ScheduledJobDto>.Failure("No JobTask found for this ScheduledJob", 404);

            scheduledJob.JobTaskId = jobTask.Id;
            scheduledJob.IsArchived = false;
            scheduledJob.CreatedAt = DateTime.UtcNow;
            scheduledJob.CreatedBy = currentUser.Id;

            await repository.AddScheduledJobAsync(scheduledJob, ct);
            var result = await repository.SaveChangesAsync(ct);
            
            if (!result)
                return Result<ScheduledJobDto>.Failure("Failed to create ScheduledJob", 400);

            var createdScheduledJob = mapper.Map<ScheduledJobDto>(scheduledJob);

            return Result<ScheduledJobDto>.Success(createdScheduledJob);
        }
    }
}
