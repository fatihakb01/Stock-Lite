using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class EditJobTask
{
    public class Command : IRequest<Result<JobTaskDto>>
    {
        public Guid Id { get; set; }
        public required EditJobTaskDto EditJobTaskDto { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<JobTaskDto>>
    {
        public async Task<Result<JobTaskDto>> Handle(Command request, CancellationToken ct)
        {
            var jobTasks = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct
            );
            var jobTask = jobTasks.SingleOrDefault();

            if (jobTask == null)
                return Result<JobTaskDto>.Failure("JobTask not found", 404);

            var dto = request.EditJobTaskDto;

            mapper.Map(dto, jobTask);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<JobTaskDto>.Failure("Failed to update JobTask", 400);

            var updatedJobTask = mapper.Map<JobTaskDto>(jobTask);

            return Result<JobTaskDto>.Success(updatedJobTask);
        }
    }
}
