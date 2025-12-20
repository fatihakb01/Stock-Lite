using Application.Entities.Core.PurchaseOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.PurchaseOrders.Validators;

public class SubmitPurchaseOrderCommandValidator
    : AbstractValidator<SubmitPurchaseOrder.Command>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required");
    }
}
