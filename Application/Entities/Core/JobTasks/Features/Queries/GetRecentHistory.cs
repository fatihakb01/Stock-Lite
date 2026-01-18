using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Queries;

public class GetRecentHistory
{
    public class Query : IRequest<Result<List<JobExecutionDto>>>
    {
        
    }

    public class Handler(
        IJobExecutionRepository repository,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<List<JobExecutionDto>>>
    {
        public async Task<Result<List<JobExecutionDto>>> Handle(Query request, CancellationToken ct)
        {
            var jobExecutions = await repository.GetRecentExecutionAsync(currentUser.CompanyId, 30, ct);

            if (!jobExecutions.Any())
                return Result<List<JobExecutionDto>>.Failure("JobExecutions not found", 404);

            var jobExecutionDtos = mapper.Map<List<JobExecutionDto>>(jobExecutions);

            return Result<List<JobExecutionDto>>.Success(jobExecutionDtos);
        }
    }
}
