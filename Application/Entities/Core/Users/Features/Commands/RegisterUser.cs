using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Entities.Core.Users.Features.Commands;

public class RegisterUser
{
    public class Command : IRequest<Result<UserDto>>
    {
        public required RegisterUserDto RegisterUserDto { get; set; }
    }

    public class Handler(
        IUserRepository userRepository, 
        IMapper mapper, 
        IConfiguration config,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.RegisterUserDto;

            // Check for existing email
            var existingUser = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingUser != null)
                return Result<UserDto>.Failure("Email is already registered.", 400);

            // Only allow username without spaces
            bool containSpaces = string.IsNullOrWhiteSpace(dto.UserName)
                || dto.UserName.Contains(' ')
                || dto.UserName.Trim() != dto.UserName;
            if (containSpaces)
                return Result<UserDto>.Failure("Username not allowed to contain spaces", 400);

            // Map DTO → Entity
            var user = mapper.Map<User>(dto);
            user.CompanyId = currentUser.CompanyId;

            // Create user in Identity
            var result = await userRepository.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<UserDto>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)), 400);

            // Generate confirmation link
            var token = await userRepository.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{config["ClientAppUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await userRepository.SendConfirmationEmailAsync(user, confirmationLink);

            // Return DTO
            var userDto = mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
    }
}