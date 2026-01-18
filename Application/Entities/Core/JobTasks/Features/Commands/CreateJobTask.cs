using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class CreateJobTask
{
    public class Command : IRequest<Result<JobTaskDto>>
    {
        public required CreateJobTaskDto CreateJobTaskDto { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<JobTaskDto>>
    {
        public async Task<Result<JobTaskDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CreateJobTaskDto;

            var jobTaskKeyExist = await repository.ExistsAsync(x =>
                x.JobTaskKey == dto.JobTaskKey,
                ct
            );

            if (jobTaskKeyExist)
                return Result<JobTaskDto>.Failure("JobTaskKey must be unique", 400);

            var jobTask = mapper.Map<JobTask>(dto);
            jobTask.CompanyId = currentUser.CompanyId;

            await repository.AddAsync(jobTask, ct);
            
            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<JobTaskDto>.Failure("Failed to create JobTask", 400);

            var createdJobTaskDto = mapper.Map<JobTaskDto>(jobTask);

            return Result<JobTaskDto>.Success(createdJobTaskDto);
        }
    }
}
