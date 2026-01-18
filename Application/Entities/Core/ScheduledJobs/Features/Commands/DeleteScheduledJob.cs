using Application.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.ScheduledJobs.Features.Commands;

public class DeleteScheduledJob
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IScheduledJobRepository repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var scheduledJobs = await repository.FindAsync(x =>
                x.Id == request.Id && x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );
            var scheduledJob = scheduledJobs.SingleOrDefault();

            if (scheduledJob == null)
                return Result<Unit>.Failure("ScheduledJob not found", 404);

            repository.Remove(scheduledJob);

            var result = await repository.SaveChangesAsync(ct);
            
            if (!result)
                return Result<Unit>.Failure("Failed to delete ScheduledJob", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
