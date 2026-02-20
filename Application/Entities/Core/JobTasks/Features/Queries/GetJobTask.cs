using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTask
{
    public class Query : IRequest<Result<JobTaskDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<JobTaskDto>>
    {
        public async Task<Result<JobTaskDto>> Handle(Query request, CancellationToken ct)
        {
            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.CompanyId == currentUser.CompanyId,
                ct,
                x => x.Jobs.OrderBy(x => x.ExecutionOrder)
            );

            if (jobTask == null)
                return Result<JobTaskDto>.Failure("JobTasks not found", 404);

            var jobTaskDtos = mapper.Map<JobTaskDto>(jobTask);

            return Result<JobTaskDto>.Success(jobTaskDtos);
        }
    }
}
