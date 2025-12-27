using Application.Entities.Core.ConfirmedOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class CreateConfirmedOrderCommandValidator : AbstractValidator<CreateConfirmedOrder.Command>
{
    public CreateConfirmedOrderCommandValidator()
    {
        RuleFor(x => x.ConfirmedOrderDto)
            .SetValidator(new BaseConfirmedOrderDtoValidator());

        RuleFor(x => x.ConfirmedOrderDto.LineItems)
            .NotEmpty()
            .WithMessage("Confirmed order must contain at least one line item");

        RuleForEach(x => x.ConfirmedOrderDto.LineItems)
            .SetValidator(new CreateConfirmedOrderLineItemDtoValidator());
    }
}
