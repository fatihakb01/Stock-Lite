using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class LoginUser
{
    public class Command : IRequest<Result<AuthResponseDto>>
    {
        public required LoginUserDto LoginUserDto { get; set; }
    }

    public class Handler(
        IUserRepository userRepository, 
        ITokenService tokenService, 
        IMapper mapper
    ) : IRequestHandler<Command, Result<AuthResponseDto>>
    {
        public async Task<Result<AuthResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.LoginUserDto;

            var user = await userRepository.GetByEmailAsync(dto.Email, ct);

            if (user == null)
                return Result<AuthResponseDto>.Failure("Invalid email.", 401);

            var passwordValid = await userRepository.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Result<AuthResponseDto>.Failure("Invalid password.", 401);

            if (!user.EmailConfirmed)
                return Result<AuthResponseDto>.Failure("Please confirm your email before logging in.", 403);

            if (!user.IsActive)
                return Result<AuthResponseDto>.Failure("Account is inactive. Please contact support.", 403);

            var token = tokenService.CreateToken(user);
            var userDto = mapper.Map<UserDto>(user);

            var response = new AuthResponseDto
            {
                User = userDto,
                Token = token
            };

            return Result<AuthResponseDto>.Success(response);
        }
    }
}
