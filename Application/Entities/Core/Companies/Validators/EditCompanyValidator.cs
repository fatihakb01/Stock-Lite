using Application.Entities.Core.Companies.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Companies.Validators;

public class EditCompanyValidator : AbstractValidator<EditCompany.Command>
{
    public EditCompanyValidator(IUniquenessChecker<Company> uniquenessChecker, IRepository<Company> repository)
    {
        // General rules
        RuleFor(x => x.CompanyDto).SetValidator(new CompanyDtoValidator());

        // Uniqueness check for Name & Email, but only if property is updated
        RuleFor(x => x.CompanyDto.Email)
            .MustAsync(async (command, email, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.Email == email) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.Email == email, ct);
            })
            .WithMessage("Company email must be unique");

        RuleFor(x => x.CompanyDto.Name)
            .MustAsync(async (command, name, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.Name == name) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.Name == name, ct);
            })           
            .WithMessage("Company name must be unique");
    }
}
