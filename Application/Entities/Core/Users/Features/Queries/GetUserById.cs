using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Users.Features.Queries;

public class GetUserById
{
    public class Query : IRequest<Result<UserDto>>
    {
        public Guid UserId { get; set; }
    }

    public class Handler(
        IUserRepository userRepository, 
        IRepository<User> repository,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query request, CancellationToken ct)
        {
            var userFromSameCompany = await repository.ExistsAsync(x =>
                x.Id == request.UserId.ToString() &&
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!userFromSameCompany)
                return Result<UserDto>.Failure("Not allowed to see user from another company", 400);

            var user = await userRepository.GetByIdAsync(request.UserId, ct);
            if (user == null)
                return Result<UserDto>.Failure("User not found.", 404);

            var userDto = mapper.Map<UserDto>(user);
            
            return Result<UserDto>.Success(userDto);
        }
    }
}
