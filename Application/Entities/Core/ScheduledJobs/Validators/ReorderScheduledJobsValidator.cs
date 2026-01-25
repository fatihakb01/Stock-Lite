using Application.Entities.Core.ScheduledJobs.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ScheduledJobs.Validators;

public class ReorderScheduledJobsValidator : AbstractValidator<ReorderScheduledJobs.Command>
{
    public ReorderScheduledJobsValidator()
    {
        RuleFor(x => x.Dto.OrderedScheduledJobIds)
            .NotEmpty()
                .WithMessage("You should have at least one ScheduledJob Id");
    }
}
