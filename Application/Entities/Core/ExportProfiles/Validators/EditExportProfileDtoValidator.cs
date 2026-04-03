using Application.Entities.Core.ExportProfiles.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ExportProfiles.Validators
{
    public class EditExportProfileDtoValidator : AbstractValidator<EditExportProfile.Command>
    {
        public EditExportProfileDtoValidator() 
        {
            RuleFor(x => x.Dto).SetValidator(new BaseExportProfileDtoValidator());
        }
    }
}
