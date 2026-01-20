using Application.Core;
using Application.Entities.Core.ScheduledJobs.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Queries;

public class GetScheduledJob
{
    public class Query : IRequest<Result<ScheduledJobDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ScheduledJobDto>>
    {
        public async Task<Result<ScheduledJobDto>> Handle(Query request, CancellationToken ct)
        {
            var scheduledJobs = await repository.FindAsync(x =>
                x.Id == request.Id &&
                x.JobTask.CompanyId == currentUser.CompanyId && 
                !x.IsArchived,
                ct
            );

            if (!scheduledJobs.Any())
                return Result<ScheduledJobDto>.Failure("scheduledJob not found", 404);

            var jobTaskDtos = mapper.Map<ScheduledJobDto>(scheduledJobs.SingleOrDefault());

            return Result<ScheduledJobDto>.Success(jobTaskDtos);
        }
    }
}
