using Application.Entities.Core.ConfirmedOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class ReceiveConfirmedOrderCommandValidator : AbstractValidator<ReceiveConfirmedOrder.Command>
{
    public ReceiveConfirmedOrderCommandValidator()
    {
        RuleFor(x => x.ReceiveDto.LineItems)
            .NotEmpty()
            .WithMessage("At least one item must be received");

        RuleForEach(x => x.ReceiveDto.LineItems)
            .SetValidator(new ReceiveConfirmedOrderItemDtoValidator());
    }
}
