using Application.Entities.Core.TransactionLineItems.DTOs;
using FluentValidation;

namespace Application.Entities.Core.TransactionLineItems.Validators;

public class TransactionLineItemDtoValidator : AbstractValidator<BaseTransactionLineItemDto>
{
    public TransactionLineItemDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .NotEmpty()
                .WithMessage("Transaction line item's quantity is required");

        RuleFor(x => x.UnitPrice)
            .NotEmpty()
                .WithMessage("Transaction line item's unit price is required")
            .Must(price => decimal.Round(price, 2) == price)
                .WithMessage("Transaction line item's unit price can only have up to 2 decimal places")
            .Must(price => price >= 0)
                .WithMessage("Transaction line item's unit price must be greater than or equal to 0");
    }
}
