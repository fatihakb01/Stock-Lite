using Application.Entities.Core.PurchaseOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class ReceivePurchaseOrderDtoValidator : AbstractValidator<ReceivePurchaseOrder.Command>
{
    public ReceivePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.ReceivePurchaseOrderDto.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required");

        RuleFor(x => x.ReceivePurchaseOrderDto.lineItems)
            .NotEmpty()
            .WithMessage("At least one item must be received");

        RuleForEach(x => x.ReceivePurchaseOrderDto.lineItems)
            .SetValidator(new ReceivePurchaseOrderItemDtoValidator());
    }
}
