using Application.Entities.Core.ScheduledJobs.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ScheduledJobs.Validators;

public class EditScheduledJobValidator : AbstractValidator<EditScheduledJob.Command>
{
    public EditScheduledJobValidator()
    {
        RuleFor(x => x.EditScheduledJobDto).SetValidator(new BaseScheduledJobDtoValidator());

        RuleFor(x => x.EditScheduledJobDto.IsArchived)
            .InclusiveBetween(false, true)
                .WithMessage("IsArchived must be set to true or false");
    }
}
