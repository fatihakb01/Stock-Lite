using Application.Entities.Core.Transactions.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.Transactions.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransaction.Command>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.TransactionDto).SetValidator(new BaseTransactionDtoValidator());

        RuleFor(x => x.TransactionDto.LineItems)
            .NotEmpty()
            .WithMessage("Transaction must contain at least one line item");

        RuleForEach(x => x.TransactionDto.LineItems)
            .SetValidator(new CreateTransactionLineItemValidator()
        );
    }
}
