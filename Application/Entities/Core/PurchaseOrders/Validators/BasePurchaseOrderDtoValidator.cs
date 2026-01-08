using Application.Entities.Core.PurchaseOrders.DTOs;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class BasePurchaseOrderDtoValidator : AbstractValidator<BasePurchaseOrderDto>
{
    public BasePurchaseOrderDtoValidator()
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
            .WithMessage("Expected delivery date cannot be before order date");
    }
}
