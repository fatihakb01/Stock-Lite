using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Users.Features.Queries;

public class GetCurrentUser
{
    public class Query : IRequest<Result<CurrentUserDto>> { }

    public class Handler(
        IUserRepository userRepository,
        IBaseCacheService<User> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<CurrentUserDto>>
    {
        public async Task<Result<CurrentUserDto>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.Id, UserKey.CurrentUser);
            var cached = await cacheService.GetAsync<CurrentUserDto>(cacheKey, ct);
            if (cached != null)
                return Result<CurrentUserDto>.Success(cached);

            var user = await userRepository.GetByIdAsync(currentUser.Id, ct);
            if (user == null)
                return Result<CurrentUserDto>.Failure("User not found.", 404);

            var currentUserDto = mapper.Map<CurrentUserDto>(user);
            await cacheService.SetAsync(cacheKey, currentUserDto, TimeSpan.FromMinutes(15), ct);
            return Result<CurrentUserDto>.Success(currentUserDto);
        }
    }
}
