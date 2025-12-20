using Application.Entities.Core.PurchaseOrders.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class UpdatePurchaseOrderDtoValidator : AbstractValidator<UpdatePurchaseOrder.Command>
{
    public UpdatePurchaseOrderDtoValidator(IUniquenessChecker<PurchaseOrder> uniquenessChecker)
    {
        RuleFor(x => x.UpdatePurchaseOrderDto).SetValidator(new BasePurchaseOrderDtoValidator(uniquenessChecker));

        RuleForEach(x => x.UpdatePurchaseOrderDto.LineItems)
            .SetValidator(new UpdatePurchaseOrderLineItemDtoValidator());
    }
}
