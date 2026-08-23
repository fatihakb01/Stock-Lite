using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
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
        IUserRepository userRepository,
        IBaseCacheService<User> cacheService,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return Result<Unit>.Failure("User not found", 404);

            var userFromSameCompany = await repository.ExistsAsync(x =>
                x.Id == request.Id.ToString() &&
                x.CompanyId == currentUser.CompanyId,
                cancellationToken
            );
            if (!userFromSameCompany)
                return Result<Unit>.Failure("Not allowed to delete user from another company", 400);

            repository.Remove(user);
            var result = await repository.SaveChangesAsync(cancellationToken);
            if (!result)
                return Result<Unit>.Failure("Failed to delete user", 400);

            await cacheService.RemoveKeysAsync(currentUser.CompanyId, [request.Id], null, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
