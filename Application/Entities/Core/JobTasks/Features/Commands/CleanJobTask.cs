using Application.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands
{
    public class CleanJobTask
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IJobTaskRepository jobTaskRepository,
            IScheduledJobRepository scheduledJobRepository,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var jobTask = await jobTaskRepository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (jobTask == null)
                    return Result<Unit>.Failure("JobTask not found", 404);

                await jobTaskRepository.CleanJobTask(request.Id, ct);
                await scheduledJobRepository.CleanScheduledJobs(request.Id, ct);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
