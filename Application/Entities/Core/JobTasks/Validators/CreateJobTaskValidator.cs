using Application.Entities.Core.JobTasks.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.JobTasks.Validators;

public class CreateJobTaskValidator : AbstractValidator<CreateJobTask.Command>
{
    public CreateJobTaskValidator() 
    {
        RuleFor(x => x.CreateJobTaskDto).SetValidator(new BaseJobTaskDtoValidator());
    }
}
