using Application.Entities.Core.ExportProfiles.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ExportProfiles.Validators
{
    public class CreateExportProfileValidator : AbstractValidator<CreateExportProfile.Command>
    {
        public CreateExportProfileValidator() 
        {
            RuleFor(x => x.Dto).SetValidator(new BaseExportProfileDtoValidator());
        }
    }
}
