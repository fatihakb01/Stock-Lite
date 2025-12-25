using Application.Entities.Core.PurchaseOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrder.Command>
{
    public CreatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.CreatePurchaseOrderDto).SetValidator(new BasePurchaseOrderDtoValidator());

        RuleFor(x => x.CreatePurchaseOrderDto.LineItems)
            .NotEmpty()
            .WithMessage("Purchase order must contain at least one line item");

        RuleForEach(x => x.CreatePurchaseOrderDto.LineItems)
            .SetValidator(new CreatePurchaseOrderLineItemDtoValidator());
    }
}
