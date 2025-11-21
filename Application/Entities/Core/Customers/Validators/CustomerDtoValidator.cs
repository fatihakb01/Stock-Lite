using Application.Entities.Core.Customers.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Customers.Validators;

public class CustomerDtoValidator : AbstractValidator<BaseCustomerDto>
{
    public CustomerDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Customer's display name is required")
            .MaximumLength(100).WithMessage("Customer's display name can only have a maximum of 100 characters");

        RuleFor(x => x.LegalName)
            .MaximumLength(100).WithMessage("Customer's legal name can only have a maximum of 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Customer's phone number can only have a maximum of 20 characters");
    }
}
