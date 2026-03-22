using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class EditJobTask
{
    public class Command : IRequest<Result<JobTaskDetailDto>>
    {
        public Guid Id { get; set; }
        public required EditJobTaskDto EditJobTaskDto { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<JobTaskDetailDto>>
    {
        public async Task<Result<JobTaskDetailDto>> Handle(Command request, CancellationToken ct)
        {
            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (jobTask == null)
                return Result<JobTaskDetailDto>.Failure("JobTask not found", 404);

            var dto = request.EditJobTaskDto;
            dto.ScheduledAt = dto.NextRunAt ?? dto.ScheduledAt;
            jobTask.AllowManualTriggerOnly = dto.ScheduledAt == null;
            jobTask.Retries = dto.NextRunAt != null ? 0 : null;
            mapper.Map(dto, jobTask);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<JobTaskDetailDto>.Failure("Failed to update JobTask", 400);

            var updatedJobTask = mapper.Map<JobTaskDetailDto>(jobTask);

            return Result<JobTaskDetailDto>.Success(updatedJobTask);
        }
    }
}
