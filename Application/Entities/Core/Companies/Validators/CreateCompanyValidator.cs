using Application.Entities.Core.Companies.Features.Commands;
using Domain.Interfaces;
using Domain.Entities.Core;
using FluentValidation;

namespace Application.Entities.Core.Companies.Validators;

public class CreateCompanyValidator : AbstractValidator<CreateCompany.Command>
{
    public CreateCompanyValidator(IUniquenessChecker<Company> uniquenessChecker)
    {
        // General rules
        RuleFor(x => x.CompanyDto).SetValidator(new CompanyDtoValidator());

        // Check for unique Name & Email
        RuleFor(x => x.CompanyDto.Email)
            .MustAsync(async (email, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.Email == email, ct))
            .WithMessage("Company email must be unique");

        RuleFor(x => x.CompanyDto.Name)
            .MustAsync(async (name, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.Name == name, ct))
            .WithMessage("Company name must be unique");
    }
}
