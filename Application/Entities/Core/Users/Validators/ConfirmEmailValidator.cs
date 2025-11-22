using Application.Entities.Core.Users.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Users.Validators;

public class ConfirmValidator : AbstractValidator<ConfirmEmailDto>
{
    public ConfirmValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required.")
            .MaximumLength(500)   
            .WithMessage("Token is too long.");
    }
}
