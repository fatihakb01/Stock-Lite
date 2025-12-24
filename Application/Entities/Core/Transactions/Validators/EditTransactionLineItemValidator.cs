using Application.Entities.Core.TransactionLineItems.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Transactions.Validators;

public class EditTransactionLineItemValidator : AbstractValidator<EditTransactionLineItemDto>
{
    public EditTransactionLineItemValidator()
    {
        
    }
}
