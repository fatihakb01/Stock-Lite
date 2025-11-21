using Application.Entities.Core.TransactionLineItems.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.TransactionLineItems.Validators;

public class CreateTransactionLineItemValidator : AbstractValidator<CreateTransactionLineItem.Command>
{
    public CreateTransactionLineItemValidator(
        IRepository<TransactionLineItem> transactionLineItemRepository,
        IRepository<Product> productRepository,
        IRepository<Transaction> transactionRepository)
    {
        RuleFor(x => x.TransactionLineItemDto).SetValidator(new TransactionLineItemDtoValidator());

        RuleFor(x => x.TransactionLineItemDto)
            .MustAsync(async (dto, ct) =>
            {
                return !await transactionLineItemRepository.ExistsAsync(
                    tl => tl.TransactionId == dto.TransactionId && tl.ProductId == dto.ProductId,
                    ct
                );
            })
            .WithMessage("This transaction already contains this product.");

        RuleFor(x => x.TransactionLineItemDto.TransactionId)
        .MustAsync(async (transactionId, ct) =>
            await transactionRepository.ExistsAsync(t => t.Id == transactionId, ct))
        .WithMessage("Transaction does not exist.");

        RuleFor(x => x.TransactionLineItemDto.ProductId)
            .MustAsync(async (productId, ct) =>
                await productRepository.ExistsAsync(p => p.Id == productId, ct))
            .WithMessage("Product does not exist.");
    }
}
