using Application.Entities.Core.JobTasks.DTOs;
using FluentValidation;

namespace Application.Entities.Core.JobTasks.Validators;

public class BaseJobTaskDtoValidator : AbstractValidator<BaseJobTaskDto>
{
    public BaseJobTaskDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("JobTask name is required")
            .MaximumLength(50)
                .WithMessage("JobTask name can only have a maximum of 50 characters"); 

        RuleFor(x => x.Description)
            .MaximumLength(200)
                .WithMessage("JobTask description can only have a maximum of 200 characters");  

        RuleFor(x => x.JobTaskKey)
            .NotEmpty()
                .WithMessage("JobTaskKey is required")
            .MaximumLength(50)
                .WithMessage("JobTaskKey can only have a maximum of 50 characters"); 
    }
}
