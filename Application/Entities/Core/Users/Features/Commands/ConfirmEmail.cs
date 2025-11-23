using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class ConfirmEmail
{
    public class Command : IRequest<Result<UserDto>>
    {
        public required ConfirmEmailDto Dto { get; set; } 
    }

    public class Handler(IUserRepository userRepository, IMapper mapper) 
        : IRequestHandler<Command, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var existingUser = await userRepository.GetByIdAsync(dto.UserId, cancellationToken);
            if (existingUser == null)
                return Result<UserDto>.Failure("User not found.", 404);

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            await userRepository.ConfirmEmailAsync(existingUser, decodedToken);
            
            var userDto = mapper.Map<UserDto>(existingUser);

            return Result<UserDto>.Success(userDto);
        }
    }
}
