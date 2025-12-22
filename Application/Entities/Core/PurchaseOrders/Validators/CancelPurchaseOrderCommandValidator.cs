using Application.Entities.Core.PurchaseOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrder.Command>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ProductId is required");
    }
}
