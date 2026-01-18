using Application.Entities.Core.JobTasks.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.JobTasks.Validators;

public class EditJobTaskValidator : AbstractValidator<EditJobTask.Command>
{
    public EditJobTaskValidator()
    {
        RuleFor(x => x.EditJobTaskDto).SetValidator(new BaseJobTaskDtoValidator());

        RuleFor(x => x.EditJobTaskDto.IsArchived)
            .InclusiveBetween(false, true)
                .WithMessage("IsArchived must be set to true or false");
    }
}
