using Application.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.JobTasks.Features.Commands;

public class DeleteJobTask
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IJobTaskRepository repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var jobTasks = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct
            );
            var jobTask = jobTasks.SingleOrDefault();

            if (jobTask == null)
                return Result<Unit>.Failure("JobTask not found", 404);

            repository.Remove(jobTask);

            var result = await repository.SaveChangesAsync(ct);
            
            if (!result)
                return Result<Unit>.Failure("Failed to delete JobTask", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
