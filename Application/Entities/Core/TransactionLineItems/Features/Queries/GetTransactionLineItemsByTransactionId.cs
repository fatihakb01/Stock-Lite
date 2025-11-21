using Application.Core;
using Application.Entities.Core.TransactionLineItems.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.TransactionLineItems.Features.Queries;

public class GetTransactionLineItemsByTransactionId
{
    public class Query : IRequest<Result<List<TransactionLineItemDto>>>
    { 
        public Guid TransactionId { get; set; }
    }

    public class Handler(IRepository<TransactionLineItem> repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<TransactionLineItemDto>>>
    {
        public async Task<Result<List<TransactionLineItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transactionLineItems = await repository
                .FindAsync(x => x.TransactionId == request.TransactionId, cancellationToken);

            if (transactionLineItems == null || !transactionLineItems.Any())
                return Result<List<TransactionLineItemDto>>
                    .Failure("No transaction line items found for this transaction", 404);

            var transactionLineItemDtos = mapper.Map<List<TransactionLineItemDto>>(transactionLineItems);

            return Result<List<TransactionLineItemDto>>.Success(transactionLineItemDtos);
        }
    }
}
