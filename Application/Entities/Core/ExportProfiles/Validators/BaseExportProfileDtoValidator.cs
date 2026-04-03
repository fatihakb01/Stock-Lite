using Application.Entities.Core.ExportProfiles.DTOs;
using FluentValidation;

namespace Application.Entities.Core.ExportProfiles.Validators
{
    public class BaseExportProfileDtoValidator : AbstractValidator<BaseExportProfileDto>
    {
        public BaseExportProfileDtoValidator() 
        { 
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Name is required")
                .MaximumLength(50)
                    .WithMessage("Maximum length is 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(200)
                    .WithMessage("Maximum length is 200 characters");

            // CK_ExportProfile_Custom_Source_Or_Export_Type:
            // exactly one of CustomSource or ExportType must be set
            RuleFor(x => x.ExportType)
                .Must((dto, exportType) =>
                    (dto.CustomSource == null) != (exportType == null))
                .WithMessage("Either CustomSource or ExportType must be filled in, but not both");

            // CK_ExportProfile_Custom_Source_And_Type:
            // CustomSource and CustomType must both be set or both be null
            RuleFor(x => x.CustomType)
                .Must((dto, customType) =>
                    (dto.CustomSource == null) == (customType == null))
                .WithMessage("CustomSource and CustomType must both be filled in or both be empty");
        }
    }
}
