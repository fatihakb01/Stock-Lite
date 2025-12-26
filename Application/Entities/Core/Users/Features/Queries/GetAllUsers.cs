using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Queries;

public class GetAllUsers
{
    public class Query : IRequest<Result<List<UserDto>>>
    {
        
    }

    public class Handler(
        IRepository<User> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<UserDto>>>
    {
        public async Task<Result<List<UserDto>>> Handle(Query request, CancellationToken ct)
        {
            var users = await repository.FindAsync(x => 
                x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (!users.Any())
                return Result<List<UserDto>>.Failure("Users not found", 404);

            var userDtos = mapper.Map<List<UserDto>>(users);
            
            return Result<List<UserDto>>.Success(userDtos);
        }
    }
}
