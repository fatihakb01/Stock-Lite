using Application.Core;
using Application.Entities.Core.TransactionLineItems.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.TransactionLineItems.Features.Queries;

public class GetTransactionLineItems
{
    public class Query : IRequest<Result<List<TransactionLineItemDto>>> { }

    public class Handler(IRepository<TransactionLineItem> repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<TransactionLineItemDto>>>
    {
        public async Task<Result<List<TransactionLineItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var transactionLineItems = await repository.GetAllAsync(cancellationToken);

            if (transactionLineItems == null)
                return Result<List<TransactionLineItemDto>>.Failure("Transaction line items not found", 404);

            var transactionLineItemDtos = mapper.Map<List<TransactionLineItemDto>>(transactionLineItems);

            return Result<List<TransactionLineItemDto>>.Success(transactionLineItemDtos);
        }
    }
}
