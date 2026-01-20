using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class DeleteUser
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<User> repository, 
        IUserRepository userRepository
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

            if (user == null)
                return Result<Unit>.Failure("User not found", 404);

            repository.Remove(user);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Failed to delete user", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
