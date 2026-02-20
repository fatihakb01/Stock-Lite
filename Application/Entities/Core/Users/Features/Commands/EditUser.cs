using Application.Core;
using Application.Entities.Core.Users.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Entities.Core.Users.Features.Commands;

public class EditUser
{
    public class Command : IRequest<Result<UserDto>>
    {
        public required Guid Id { get; set; }
        public required EditUserDto EditUserDto { get; set; }
    }

    public class Handler(
        IUserRepository userRepository,
        IRepository<User> repository,
        IMapper mapper,
        IConfiguration config,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userFromSameCompany = await repository.ExistsAsync(x =>
                x.Id == request.Id.ToString() &&
                x.CompanyId == currentUser.CompanyId,
                cancellationToken
            );

            if (!userFromSameCompany)
                return Result<UserDto>.Failure("Not allowed to edit user from another company", 400);

            var dto = request.EditUserDto;

            var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return Result<UserDto>.Failure("User not found.", 404);

            bool containSpaces = string.IsNullOrWhiteSpace(dto.UserName)
                || dto.UserName.Contains(' ')
                || dto.UserName.Trim() != dto.UserName;

            if (containSpaces)
                return Result<UserDto>.Failure("Username not allowed to contain spaces", 400);

            // Detect if email is changing
            bool emailChanged = !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);

            if (emailChanged)
            {
                // Is new email already used by another user?
                var existingWithEmail = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);

                if (existingWithEmail != null && existingWithEmail.Id != user.Id)
                {
                    return Result<UserDto>.Failure("Email is already registered.", 400);
                }

                // Send a new confirmation link
                var token = await userRepository.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink =
                    $"{config["ClientAppUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await userRepository.SendConfirmationEmailAsync(user, confirmationLink);

                // Reset confirmation status since the email changed
                user.EmailConfirmed = false;
            }

            mapper.Map(dto, user);

            var saved = await repository.SaveChangesAsync(cancellationToken);
            if (!saved)
                return Result<UserDto>.Failure("Failed to update user", 400);

            var updatedDto = mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(updatedDto);
        }
    }
}
