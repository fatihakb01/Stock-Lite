using Application.Core;
using Application.Entities.Users.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Users.Features.Queries;

public class GetUserById
{
    public class Query : IRequest<Result<UserDto>>
    {
        public Guid UserId { get; set; }
    }

    public class Handler(IUserRepository userRepository, IMapper mapper)
        : IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
                return Result<UserDto>.Failure("User not found.", 404);

            var userDto = mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
    }
}
