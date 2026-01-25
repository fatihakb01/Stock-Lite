using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Queries;

public class GetScheduledJobHistory
{
    public class Query : IRequest<Result<IReadOnlyList<JobExecutionDto>>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<IReadOnlyList<JobExecutionDto>>>
    {
        public async Task<Result<IReadOnlyList<JobExecutionDto>>> Handle(Query request, CancellationToken ct)
        {
            var executions = await repository
                .GetScheduledJobAndExecutionsAsync(request.Id, currentUser.CompanyId, ct);

            if (!executions.Any())
                return Result<IReadOnlyList<JobExecutionDto>>
                    .Failure("No executions found", 404);

            var dto = executions.Select(e => new JobExecutionDto
            {
                JobTaskName = e.ScheduledJob.JobTask.Name,
                ScheduledJobName = e.ScheduledJob.Name,
                Status = e.Status,
                StartedAt = e.StartedAt,
                FinishedAt = e.FinishedAt,
                ErrorMessage = e.ErrorMessage
            }).ToList();

            return Result<IReadOnlyList<JobExecutionDto>>.Success(dto);
        }
    }
}
