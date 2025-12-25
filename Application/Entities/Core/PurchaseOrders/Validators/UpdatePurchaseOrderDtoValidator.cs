using Application.Entities.Core.PurchaseOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class UpdatePurchaseOrderDtoValidator : AbstractValidator<UpdatePurchaseOrder.Command>
{
    public UpdatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.UpdatePurchaseOrderDto).SetValidator(new BasePurchaseOrderDtoValidator());

        RuleForEach(x => x.UpdatePurchaseOrderDto.LineItems)
            .SetValidator(new UpdatePurchaseOrderLineItemDtoValidator());
    }
}
