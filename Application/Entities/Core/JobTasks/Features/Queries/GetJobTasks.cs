using Application.Core;
using Application.Entities.Core.JobTasks.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetJobTasks
{
    public class Query : IRequest<Result<List<JobTaskDto>>>
    {
        
    }

    public class Handler(
        IJobTaskRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<JobTaskDto>>>
    {
        public async Task<Result<List<JobTaskDto>>> Handle(Query request, CancellationToken ct)
        {
            var jobTasks = await repository.FindAsync(x =>
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!jobTasks.Any())
                return Result<List<JobTaskDto>>.Failure("JobTasks not found", 404);

            jobTasks = jobTasks.OrderBy(j => j.NextRunAt);
            var jobTaskDtos = mapper.Map<List<JobTaskDto>>(jobTasks);

            return Result<List<JobTaskDto>>.Success(jobTaskDtos);
        }
    }
}
