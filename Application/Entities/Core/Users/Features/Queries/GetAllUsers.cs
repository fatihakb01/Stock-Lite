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

public class GetAllUsers
{
    public class Query : IRequest<Result<List<UserDto>>>
    {
        
    }

    public class Handler(
        IRepository<User> repository,
        IBaseCacheService<User> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<UserDto>>>
    {
        public async Task<Result<List<UserDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.CompanyId, UserKey.Users);
            var cached = await cacheService.GetAsync<List<UserDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<UserDto>>.Success(cached);

            var users = await repository.FindAsync(x => 
                x.CompanyId == currentUser.CompanyId, 
                ct
            );
            if (!users.Any())
                return Result<List<UserDto>>.Failure("Users not found", 404);

            var userDtos = mapper.Map<List<UserDto>>(users);
            await cacheService.SetAsync(cacheKey, userDtos, TimeSpan.FromMinutes(5), ct);
            return Result<List<UserDto>>.Success(userDtos);
        }
    }
}
