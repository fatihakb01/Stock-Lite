using Application.Entities.Core.Transactions.Features.Commands;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Transactions.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransaction.Command>
{
    public CreateTransactionValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.TransactionDto).SetValidator(new TransactionDtoValidator(clock));
    }
}
