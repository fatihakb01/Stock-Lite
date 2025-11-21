using Application.Entities.Core.Transactions.DTOs;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Transactions.Validators;

public class TransactionDtoValidator : AbstractValidator<BaseTransactionDto>
{
    public TransactionDtoValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.TotalAmount)
            .NotEmpty()
                .WithMessage("Transaction's total amount is required")
            .Must(amount => decimal.Round(amount, 2) == amount)
                .WithMessage("Transaction's total amount can only have up to 2 decimal places");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
                .WithMessage("Transaction date is required")
            .Must(date => date.ToUniversalTime() <= clock.UtcNow)
                .WithMessage("Transaction date must be in the past or equal to now (UTC)");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("Transaction's payment method can only have a maximum of 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(200).WithMessage("Transaction's notes can only have a maximum of 200 characters");
    }
}
