using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Queries;

public class GetScheduledJobHistory
{
    public class Query : IRequest<Result<ScheduledJobAndExecutionsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ScheduledJobAndExecutionsDto>>
    {
        public async Task<Result<ScheduledJobAndExecutionsDto>> Handle(Query request, CancellationToken ct)
        {
            var scheduledJobs = await repository.FindAsync(x =>
                x.Id == request.Id &&
                x.JobTask.CompanyId == currentUser.CompanyId && 
                !x.IsArchived,
                ct
            );

            if (!scheduledJobs.Any())
                return Result<ScheduledJobAndExecutionsDto>.Failure("scheduledJob not found", 404);

            var scheduledJobExecutions = await repository
                .GetScheduledJobAndExecutionsAsync(request.Id, ct);

            var scheduledJobExecutionsDto = mapper
                .Map<ScheduledJobAndExecutionsDto>(scheduledJobExecutions);

            return Result<ScheduledJobAndExecutionsDto>.Success(scheduledJobExecutionsDto);
        }
    }
}
