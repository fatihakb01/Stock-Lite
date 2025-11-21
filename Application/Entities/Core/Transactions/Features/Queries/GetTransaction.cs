using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransaction
{
    public class Query : IRequest<Result<TransactionDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(IRepository<Transaction> repository, IMapper mapper)
        : IRequestHandler<Query, Result<TransactionDto>>
    {
        public async Task<Result<TransactionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transaction = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (transaction == null)
                return Result<TransactionDto>.Failure("Transaction not found", 404);

            var transactionDto = mapper.Map<TransactionDto>(transaction);

            return Result<TransactionDto>.Success(transactionDto);
        }
    }
}
