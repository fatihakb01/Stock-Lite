using Application.Entities.Core.ConfirmedOrders.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class UpdateConfirmedOrderCommandValidator : AbstractValidator<UpdateConfirmedOrder.Command>
{
    public UpdateConfirmedOrderCommandValidator()
    {
        RuleFor(x => x.ConfirmedOrderDto)
            .SetValidator(new BaseConfirmedOrderDtoValidator());

        RuleForEach(x => x.ConfirmedOrderDto.LineItems)
            .SetValidator(new UpdateConfirmedOrderLineItemDtoValidator());
    }
}
