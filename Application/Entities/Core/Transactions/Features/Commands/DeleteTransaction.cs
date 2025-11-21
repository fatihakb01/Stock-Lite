using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class DeleteTransaction
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IRepository<Transaction> repository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (transaction == null)
                return Result<Unit>.Failure("Transaction not found", 404);

            repository.Remove(transaction);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Failed to delete transaction", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
