using Application.Entities.Core.Companies.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.Companies.Validators;

public class CreateCompanyValidator : AbstractValidator<CreateCompany.Command>
{
    public CreateCompanyValidator()
    {
        // General rules
        RuleFor(x => x.CompanyDto).SetValidator(new CompanyDtoValidator());
    }
}
