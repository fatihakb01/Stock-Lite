using Application.Entities.Core.PurchaseOrders.DTOs;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class CreatePurchaseOrderLineItemDtoValidator : AbstractValidator<CreatePurchaseOrderLineItemDto>
{
    public CreatePurchaseOrderLineItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit cost must be greater than or equal to 0.00");
    }
}
