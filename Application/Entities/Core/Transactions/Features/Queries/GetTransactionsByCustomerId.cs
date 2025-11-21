using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransactionsByCustomerId
{
    public class Query : IRequest<Result<List<TransactionDto>>>
    { 
        public Guid CustomerId { get; set; }
    }

    public class Handler(IRepository<Transaction> repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<TransactionDto>>>
    {
        public async Task<Result<List<TransactionDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transactions = await repository
                .FindAsync(x => x.CustomerId == request.CustomerId, cancellationToken);

            if (transactions == null || !transactions.Any())
                return Result<List<TransactionDto>>.Failure("No transactions found for this customer", 404);

            var transactionDtos = mapper.Map<List<TransactionDto>>(transactions);

            return Result<List<TransactionDto>>.Success(transactionDtos);
        }
    }
}
