using Application.Entities.Core.Customers.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Customers.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomer.Command>
{
    public CreateCustomerValidator(IUniquenessChecker<Customer> uniquenessChecker)
    {
        // General rules
        RuleFor(x => x.CustomerDto).SetValidator(new CustomerDtoValidator());

        // Check for unique DisplayName, LegalName and Email
        RuleFor(x => x.CustomerDto.DisplayName)
            .MustAsync(async (displayName, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.DisplayName == displayName, ct))
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.DisplayName))
            .WithMessage("Company's display name must be unique");

        RuleFor(x => x.CustomerDto.LegalName)
            .MustAsync(async (legalName, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.LegalName == legalName, ct))
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.LegalName))
            .WithMessage("Company's legal name must be unique");

        RuleFor(x => x.CustomerDto.Email)
            .MustAsync(async (email, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.Email == email, ct))
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.Email))
            .WithMessage("Company email must be unique");
    }
}
