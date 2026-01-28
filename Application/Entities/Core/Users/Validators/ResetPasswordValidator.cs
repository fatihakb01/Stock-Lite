using Application.Entities.Core.Users.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Users.Validators;

public class ResetPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
