using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces;
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
            var jobTasks = await repository.FindAsync(x =>
                x.Id == request.Id &&
                x.CompanyId == currentUser.CompanyId && 
                !x.IsArchived,
                ct,
                x => x.Jobs
            );

            if (!jobTasks.Any())
                return Result<JobTaskDto>.Failure("JobTasks not found", 404);

            var jobTaskDtos = mapper.Map<JobTaskDto>(jobTasks.SingleOrDefault());

            return Result<JobTaskDto>.Success(jobTaskDtos);
        }
    }
}
