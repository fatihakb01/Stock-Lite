using Application.Entities.Core.ScheduledJobs.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ScheduledJobs.Validators;

public class CreateScheduledJobValidator : AbstractValidator<CreateScheduledJob.Command>
{
    public CreateScheduledJobValidator()
    {
        RuleFor(x => x.CreateScheduledJobDto).SetValidator(new BaseScheduledJobDtoValidator());

        RuleFor(x => x.CreateScheduledJobDto.JobTaskKey)
            .NotEmpty()
                .WithMessage("JobTaskKey is required")
            .MaximumLength(50)
                .WithMessage("JobTaskKey can only have a maximum of 50 characters"); 
    }
}
