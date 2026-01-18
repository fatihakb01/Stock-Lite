using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Interfaces;
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
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Command, Result<ScheduledJobDto>>
    {
        public async Task<Result<ScheduledJobDto>> Handle(Command request, CancellationToken ct)
        {
            var scheduledJobs = await repository.FindAsync(x =>
                x.Id == request.Id && x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );
            var scheduledJob = scheduledJobs.SingleOrDefault();

            if (scheduledJob == null)
                return Result<ScheduledJobDto>.Failure("ScheduledJob not found", 404);

            var dto = request.EditScheduledJobDto;

            var jobTasks = await jobTaskRepo.FindAsync(x =>
                x.JobTaskKey == dto.JobTaskKey,
                ct
            );

            if (!jobTasks.Any())
                return Result<ScheduledJobDto>.Failure("JobTaskKey not found", 404);

            var jobTaskId = jobTasks.Select(x => x.Id).SingleOrDefault();

            var scheduledJobPositionExist = await repository.ExistsAsync(x =>
                x.Id != request.Id &&
                x.JobTaskId == jobTaskId && 
                x.ExecutionOrder == dto.ExecutionOrder,
                ct
            );

            if (scheduledJobPositionExist)
                return Result<ScheduledJobDto>.Failure("Not allowed to use an existing ExecutionOrder.", 404);

            scheduledJob.JobTaskId = jobTaskId;

            mapper.Map(dto, scheduledJob);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<ScheduledJobDto>.Failure("Failed to update ScheduledJob", 400);

            var updatedScheduledJob = mapper.Map<ScheduledJobDto>(scheduledJob);

            return Result<ScheduledJobDto>.Success(updatedScheduledJob);
        }
    }
}
