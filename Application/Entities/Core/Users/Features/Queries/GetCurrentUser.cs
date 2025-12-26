using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Queries;

public class GetCurrentUser
{
    public class Query : IRequest<Result<UserDto>> { }

    public class Handler(
        IUserRepository userRepository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query request, CancellationToken ct)
        {
            var user = await userRepository.GetByIdAsync(currentUser.Id, ct);
            if (user == null)
                return Result<UserDto>.Failure("User not found.", 404);

            var userDto = mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(userDto);
        }
    }
}
