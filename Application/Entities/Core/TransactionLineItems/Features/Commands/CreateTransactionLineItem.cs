using Application.Core;
using Application.Entities.Core.TransactionLineItems.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.TransactionLineItems.Features.Commands;

public class CreateTransactionLineItem
{
    public class Command : IRequest<Result<TransactionLineItemDto>>
    {
        public required CreateTransactionLineItemDto TransactionLineItemDto { get; set; }
    }

    public class Handler(IRepository<TransactionLineItem> repository, IMapper mapper)
        : IRequestHandler<Command, Result<TransactionLineItemDto>>
    {
        public async Task<Result<TransactionLineItemDto>>
            Handle(Command request, CancellationToken cancellationToken)
        {
            var transactionLineItem = mapper.Map<TransactionLineItem>(request.TransactionLineItemDto);

            await repository.AddAsync(transactionLineItem, cancellationToken);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<TransactionLineItemDto>
                    .Failure("Failed to create transaction line item", 400);

            var createdTransactionLineItemDto = mapper
                .Map<TransactionLineItemDto>(transactionLineItem);

            return Result<TransactionLineItemDto>.Success(createdTransactionLineItemDto);
        }
    }
}
