using Application.Core;
using Application.Entities.Users.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Entities.Users.Features.Queries;

public class GetCurrentUser
{
    public class Query : IRequest<Result<UserDto>> { }

    public class Handler(
        IUserRepository userRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Result<UserDto>.Failure("User is not authenticated.", 401);

            var user = await userRepository.GetByIdAsync(Guid.Parse(userId), cancellationToken);
            if (user == null)
                return Result<UserDto>.Failure("User not found.", 404);

            var userDto = mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
    }
}
