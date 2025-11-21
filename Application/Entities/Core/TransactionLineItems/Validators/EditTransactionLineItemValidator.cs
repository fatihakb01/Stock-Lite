using Application.Entities.Core.TransactionLineItems.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.TransactionLineItems.Validators;

public class EditTransactionLineItemValidator : AbstractValidator<EditTransactionLineItem.Command>
{
    public EditTransactionLineItemValidator()
    {
        RuleFor(x => x.TransactionLineItemDto).SetValidator(new TransactionLineItemDtoValidator());
    }
}
