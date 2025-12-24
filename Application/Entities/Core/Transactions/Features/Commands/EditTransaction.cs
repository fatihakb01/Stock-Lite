using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class EditTransaction
{
    public class Command : IRequest<Result<TransactionDto>>
    {
        public Guid Id { get; set; }
        public required EditTransactionDto TransactionDto { get; set; }
    }

    public class Handler(
        IRepository<Transaction> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<TransactionDto>>
    {
        public async Task<Result<TransactionDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.TransactionDto;

            var transactions = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct,
                t => t.LineItems
            );

            var transaction = transactions.SingleOrDefault();

            if (transaction == null)
                return Result<TransactionDto>.Failure("Transaction not found", 404);

            if (transaction.TransactionId != dto.TransactionId)
                return Result<TransactionDto>.Failure("Transaction Id cannot be changed", 400);

            transaction.TotalAmount = dto.LineItems.Sum(x => x.Quantity * x.UnitPrice);

            mapper.Map(dto, transaction);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<TransactionDto>.Failure("Failed to update transaction", 400);

            var updatedTransactionDto = mapper.Map<TransactionDto>(transaction);

            return Result<TransactionDto>.Success(updatedTransactionDto);
        }
    }
}
