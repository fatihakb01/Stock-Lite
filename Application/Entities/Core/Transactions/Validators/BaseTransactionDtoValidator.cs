using Application.Entities.Core.Transactions.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Transactions.Validators;

public class BaseTransactionDtoValidator : AbstractValidator<BaseTransactionDto>
{
    public BaseTransactionDtoValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
                .WithMessage("Transaction Id is required")
            .MaximumLength(50)
                .WithMessage("Transaction Id can only have a maximum of 50 characters");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
                .WithMessage("Transaction date is required")
            .Must(date => date.ToUniversalTime() <= DateTime.UtcNow)
                .WithMessage("Transaction date must be in the past or equal to now (UTC)");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("Transaction's payment method can only have a maximum of 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(200).WithMessage("Transaction's notes can only have a maximum of 200 characters");
    }
}
