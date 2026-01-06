using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransactionsByProductId
{
    public class Query : IRequest<Result<List<TransactionDto>>>
    {
        public required Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<Transaction> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<TransactionDto>>>
    {
        public async Task<Result<List<TransactionDto>>> Handle(Query request, CancellationToken ct)
        {
            var transactions = await repository.FindAsync(
                t =>
                    t.CompanyId == currentUser.CompanyId &&
                    t.LineItems.Any(li => li.ProductId == request.ProductId),
                ct,
                t => t.LineItems.Where(li => li.ProductId == request.ProductId)
            );

            if (!transactions.Any())
                return Result<List<TransactionDto>>.Failure("No transactions found for this product", 404);

            var transactionDto = mapper.Map<List<TransactionDto>>(transactions);

            return Result<List<TransactionDto>>.Success(transactionDto);
        }
    }
}
