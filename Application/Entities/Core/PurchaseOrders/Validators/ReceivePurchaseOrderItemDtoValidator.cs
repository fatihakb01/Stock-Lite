using Application.Entities.Core.PurchaseOrders.DTOs;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class ReceivePurchaseOrderItemDtoValidator : AbstractValidator<ReceivePurchaseOrderItemDto>
{
    public ReceivePurchaseOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.QuantityReceived)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity received must be greater than 0");
    }
}
