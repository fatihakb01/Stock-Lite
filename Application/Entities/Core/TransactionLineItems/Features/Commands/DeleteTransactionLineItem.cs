using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.TransactionLineItems.Features.Commands;

public class DeleteTransactionLineItem
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IRepository<TransactionLineItem> repository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transactionLineItem = await repository
                .GetByIdAsync(request.Id, cancellationToken);

            if (transactionLineItem == null)
                return Result<Unit>.Failure("Transaction line item not found", 404);

            repository.Remove(transactionLineItem);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Failed to delete transaction line item", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
