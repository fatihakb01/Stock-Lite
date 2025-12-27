using Application.Entities.Core.ConfirmedOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class CancelConfirmedOrderCommandValidator : AbstractValidator<CancelConfirmedOrder.Command>
{
    public CancelConfirmedOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}
