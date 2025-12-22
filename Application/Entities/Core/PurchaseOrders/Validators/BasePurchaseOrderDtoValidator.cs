using Application.Entities.Core.PurchaseOrders.DTOs;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class BasePurchaseOrderDtoValidator : AbstractValidator<BasePurchaseOrderDto>
{
    public BasePurchaseOrderDtoValidator(IUniquenessChecker<PurchaseOrder> uniquenessChecker)
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Maximum of 50 characters exceeded");

        RuleFor(x => x.OrderNumber)
            .MustAsync(async (orderNumber, ct) =>
                await uniquenessChecker.IsUniqueAsync(c => c.OrderNumber == orderNumber, ct))
            .When(x => !string.IsNullOrWhiteSpace(x.OrderNumber))
            .WithMessage("Order number must be unique");

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Order date cannot be in the future");

        RuleFor(x => x.ExpectedDeliveryDate)
            .GreaterThanOrEqualTo(x => x.OrderDate)
            .When(x => x.ExpectedDeliveryDate.HasValue)
            .WithMessage("Expected delivery date cannot be before order date");
    }
}
