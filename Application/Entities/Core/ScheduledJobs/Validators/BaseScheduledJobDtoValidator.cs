using Application.Entities.Core.ScheduledJobs.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Entities.Core.ScheduledJobs.Validators;

public class BaseScheduledJobDtoValidator : AbstractValidator<BaseScheduledJobDto>
{
    private const string jobTypeMessage = @"JobType can't be empty when defining a job.
    JobType must be one of the following: 0 (Import), 1 (StandardCalculation), 2 (CustomCalculation).";

    private const string importDataTypeMessage = @"ImportDataType can't be empty when JobType equals Import.
    ImportDataType must be one of the following: 0 (Products), 1 (Transactions), 2 (ConfirmedOrders), 3 (PurchaseOrders).";

    private const string calculationTypeMessage = @"CalculationType can't be empty when JobType equals StandardCalculation.
    CalculationType must be one of the following: 0 (StockLevels), 1 (ConfirmedOrderAggregates), 2 (Forecasts), 3 (SalesAggregates), 4 (PurchaseForecasts).";
    
    public BaseScheduledJobDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is required")
            .MaximumLength(50)
                .WithMessage("Name can only have a maximum of 50 characters"); 

        RuleFor(x => x.Description)
            .MaximumLength(200)
                .WithMessage("Description can only have a maximum of 200 characters");

        RuleFor(x => x.ExecutionOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.JobType)
            .NotNull()
                .WithMessage(jobTypeMessage);


        When(x => x.JobType == JobType.Import, () =>
        {
            RuleFor(x => x.ImportDataType)
                .NotNull()
                    .WithMessage(importDataTypeMessage);
            RuleFor(x => x.Source)
                .NotEmpty()
                    .WithMessage("Source can't be empty when JobType equals Import.");
        });

        When(x => x.JobType == JobType.StandardCalculation, () =>
        {
            RuleFor(x => x.CalculationType)
                .NotNull()
                    .WithMessage(calculationTypeMessage);
        });

        When(x => x.JobType == JobType.CustomCalculation, () =>
        {
            RuleFor(x => x.Target)
                .NotEmpty()
                    .WithMessage("Target can't be empty when JobType equals CustomCalculation.");
        });
    }
}
