using Application.Core;
using Application.Entities.Core.Users.DTOs;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Entities.Core.Users.Features.Commands;

public class ResetPassword
{
    public class Command : IRequest<Result<Unit>>
    {
        public required ForgotPasswordDto Dto { get; set; } 
    }

    public class Handler(
        IUserRepository userRepository, 
        IConfiguration config
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var user = await userRepository.GetByEmailAsync(request.Dto.Email, ct);
            if (user == null)
                return Result<Unit>.Failure("Email not found.", 404);

            var token = await userRepository.GeneratePasswordResetTokenAsync(user);

            var link = $"{config["ClientAppUrl"]}/change-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await userRepository.SendPasswordResetEmailAsync(user, link);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
