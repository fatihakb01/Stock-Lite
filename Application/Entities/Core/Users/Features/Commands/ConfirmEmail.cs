using Application.Core;
using Application.Entities.Core.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class ConfirmEmail
{
    public class Command : IRequest<Result<Unit>>
    {
        public required ConfirmEmailDto Dto { get; set; } 
    }

    public class Handler(IUserRepository userRepository) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var existingUser = await userRepository.GetByIdAsync(dto.UserId, cancellationToken);
            if (existingUser == null)
                return Result<Unit>.Failure("User not found.", 404);

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            await userRepository.ConfirmEmailAsync(existingUser, decodedToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
