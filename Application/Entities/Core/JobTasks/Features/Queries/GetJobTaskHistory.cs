using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTaskHistory
{
    public class Query : IRequest<Result<JobTaskAndExecutionsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<JobTaskAndExecutionsDto>>
    {
        public async Task<Result<JobTaskAndExecutionsDto>> Handle(Query request, CancellationToken ct)
        {
            var jobTask = await repository
                .GetJobAndExecutionsAsync(request.Id, ct);

            if (jobTask == null || jobTask.CompanyId != currentUser.CompanyId)
                return Result<JobTaskAndExecutionsDto>.Failure("JobTask not found", 404);

            var jobTaskDto = mapper.Map<JobTaskAndExecutionsDto>(jobTask);

            return Result<JobTaskAndExecutionsDto>.Success(jobTaskDto);
        }
    }
}
