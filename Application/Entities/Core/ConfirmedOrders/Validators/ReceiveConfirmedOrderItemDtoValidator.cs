using Application.Entities.Core.ConfirmedOrders.DTOs;
using FluentValidation;

namespace Application.Entities.Core.ConfirmedOrders.Validators;

public class ReceiveConfirmedOrderItemDtoValidator : AbstractValidator<ReceiveConfirmedOrderItemDto>
{
    public ReceiveConfirmedOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.QuantitySold)
            .GreaterThan(0)
            .WithMessage("Quantity sold must be greater than 0");
    }
}
