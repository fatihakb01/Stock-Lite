using Application.Core;
using Application.Entities.Core.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Users.Features.Commands;

public class ConfirmEmail
{
    public class Command : IRequest<Result<ConfirmEmailResult>>
    {
        public required ConfirmEmailDto Dto { get; set; } 
    }

    public class Handler(IUserRepository userRepository) 
        : IRequestHandler<Command, Result<ConfirmEmailResult>>
    {
        public async Task<Result<ConfirmEmailResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var user = await userRepository.GetByIdAsync(dto.UserId, cancellationToken);

            if (user == null)
                return Result<ConfirmEmailResult>.Failure("User not found.", 404);

            bool wasConfirmedBefore = user.EmailConfirmed;

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            await userRepository.ConfirmEmailAsync(user, decodedToken);
            
            return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult
            {
                Email = user.Email!,
                AlreadyConfirmed = wasConfirmedBefore
            });
        }
    }
}
