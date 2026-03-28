using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Users.Features.Queries;

public class GetCurrentUser
{
    public class Query : IRequest<Result<CurrentUserDto>> { }

    public class Handler(
        IUserRepository userRepository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<CurrentUserDto>>
    {
        public async Task<Result<CurrentUserDto>> Handle(Query request, CancellationToken ct)
        {
            var user = await userRepository.GetByIdAsync(currentUser.Id, ct);
            if (user == null)
                return Result<CurrentUserDto>.Failure("User not found.", 404);

            var currentUserDto = mapper.Map<CurrentUserDto>(user);

            return Result<CurrentUserDto>.Success(currentUserDto);
        }
    }
}
