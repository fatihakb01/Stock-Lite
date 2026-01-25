using Application.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
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
            var scheduledJobs = await repository.ExistsAsync(x =>
                x.Id == request.Id && x.JobTask.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!scheduledJobs)
                return Result<Unit>.Failure("ScheduledJob not found", 404);

            await repository.DeleteScheduledJobAsync(request.Id, ct);
            var result = await repository.SaveChangesAsync(ct);
            
            if (!result)
                return Result<Unit>.Failure("Failed to delete ScheduledJob", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
