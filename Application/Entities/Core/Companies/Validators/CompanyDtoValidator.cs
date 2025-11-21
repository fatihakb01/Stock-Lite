using Application.Entities.Core.Companies.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Companies.Validators;

public class CompanyDtoValidator : AbstractValidator<BaseCompanyDto>
{
    public CompanyDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(100).WithMessage("Company name can only have a maximum of 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(250)
            .WithMessage("Company address can only have a maximum of 250 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("Company phone number can only have a maximum of 20 characters");
    }    
}
