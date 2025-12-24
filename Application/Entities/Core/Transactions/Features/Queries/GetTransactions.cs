using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransactions
{
    public class Query : IRequest<Result<List<TransactionDto>>> { }

    public class Handler(
        IRepository<Transaction> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<TransactionDto>>>
    {
        public async Task<Result<List<TransactionDto>>> Handle(Query request, CancellationToken ct)
        {
            var transactions = await repository.FindAsync(x =>
                x.CompanyId == currentUser.CompanyId,
                ct,
                t => t.LineItems
            );

            if (!transactions.Any())
                return Result<List<TransactionDto>>.Failure("Transactions not found", 404);

            var transactionDtos = mapper.Map<List<TransactionDto>>(transactions);

            return Result<List<TransactionDto>>.Success(transactionDtos);
        }
    }
}
