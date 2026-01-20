using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class DeleteTransaction
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<Transaction> repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var transactions = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct,
                t => t.LineItems
            );

            var transaction = transactions.SingleOrDefault();

            if (transaction == null)
                return Result<Unit>.Failure("Transaction not found", 404);

            repository.Remove(transaction);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<Unit>.Failure("Failed to delete transaction", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
