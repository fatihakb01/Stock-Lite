using Application.Entities.Core.Customers.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Customers.Validators;

public class EditCustomerValidator : AbstractValidator<EditCustomer.Command>
{
    public EditCustomerValidator(IUniquenessChecker<Customer> uniquenessChecker, IRepository<Customer> repository)
    {
        // General rules
        RuleFor(x => x.CustomerDto).SetValidator(new CustomerDtoValidator());

        // Uniqueness check for DisplayName, LegalName and Email, but only if property is updated
        RuleFor(x => x.CustomerDto.DisplayName)
            .MustAsync(async (command, displayName, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.DisplayName == displayName) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.DisplayName == displayName, ct);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.DisplayName))
            .WithMessage("Company's display name must be unique");

        RuleFor(x => x.CustomerDto.LegalName)
            .MustAsync(async (command, legalName, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.LegalName == legalName) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.LegalName == legalName, ct);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.LegalName))
            .WithMessage("Company's legal name must be unique");

        RuleFor(x => x.CustomerDto.Email)
            .MustAsync(async (command, email, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.Email == email) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.Email == email, ct);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerDto.Email))
            .WithMessage("Company email must be unique");
    }
}
