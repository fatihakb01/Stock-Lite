using Application.Entities.Core.ConfirmedOrders.DTOs;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class BaseConfirmedOrderDtoValidator : AbstractValidator<BaseConfirmedOrderDto>
{
    public BaseConfirmedOrderDtoValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Maximum of 50 characters exceeded");

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Order date cannot be in the future");

        RuleFor(x => x.ExpectedDeliveryDate)
            .GreaterThanOrEqualTo(x => x.OrderDate)
            .When(x => x.ExpectedDeliveryDate.HasValue)
            .WithMessage("Expected delivery date cannot be before order date");
    }
}
