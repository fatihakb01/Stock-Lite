using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTask
{
    public class Query : IRequest<Result<JobTaskDetailDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<JobTaskDetailDto>>
    {
        public async Task<Result<JobTaskDetailDto>> Handle(Query request, CancellationToken ct)
        {
            var jobTask = await repository.SingleOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.CompanyId == currentUser.CompanyId,
                ct,
                x => x.Jobs.OrderBy(x => x.ExecutionOrder)
            );

            if (jobTask == null)
                return Result<JobTaskDetailDto>.Failure("JobTasks not found", 404);

            var jobTaskDtos = mapper.Map<JobTaskDetailDto>(jobTask);

            return Result<JobTaskDetailDto>.Success(jobTaskDtos);
        }
    }
}
