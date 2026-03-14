using Application.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands
{
    public class CleanScheduledJob
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IScheduledJobRepository scheduledJobRepository,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var scheduledJob = await scheduledJobRepository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.JobTask.CompanyId == currentUser.CompanyId,
                    ct,
                    x => x.JobTaskId
                );

                if (scheduledJob == null)
                    return Result<Unit>.Failure("ScheduledJob not found", 404);

                await scheduledJobRepository.CleanScheduledJob(scheduledJob.JobTaskId, ct);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
