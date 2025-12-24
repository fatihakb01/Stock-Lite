using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransactionById
{
    public class Query : IRequest<Result<TransactionDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(
        IRepository<Transaction> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<TransactionDto>>
    {
        public async Task<Result<TransactionDto>> Handle(Query request, CancellationToken ct)
        {
            var transactions = await repository.FindAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct,
                t => t.LineItems
            );

            var transaction = transactions.SingleOrDefault();

            if (transaction == null)
                return Result<TransactionDto>.Failure("Transaction not found", 404);

            var transactionDto = mapper.Map<TransactionDto>(transaction);

            return Result<TransactionDto>.Success(transactionDto);
        }
    }
}
