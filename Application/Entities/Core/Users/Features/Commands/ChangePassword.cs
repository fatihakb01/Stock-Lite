using Application.Core;
using Application.Entities.Core.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class ChangePassword
{
    public class Command : IRequest<Result<Unit>>
    {
        public required ChangePasswordDto Dto { get; set; }
    }

    public class Handler(
        IUserRepository userRepository
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.Dto;

            var user = await userRepository.GetByIdAsync(dto.UserId, ct);
            if (user == null)
                return Result<Unit>.Failure("User not found.", 404);

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await userRepository.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (!result.Succeeded)
                return Result<Unit>.Failure("Could not reset password.", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
