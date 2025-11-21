using Application.Core;
using Application.Entities.Core.TransactionLineItems.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.TransactionLineItems.Features.Commands;

public class EditTransactionLineItem
{
    public class Command : IRequest<Result<TransactionLineItemDto>>
    {
        public Guid Id { get; set; }
        public required EditTransactionLineItemDto TransactionLineItemDto { get; set; }
    }

    public class Handler(IRepository<TransactionLineItem> repository, IMapper mapper)
        : IRequestHandler<Command, Result<TransactionLineItemDto>>
    {
        public async Task<Result<TransactionLineItemDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transactionLineItem = await repository
                .GetByIdAsync(request.Id, cancellationToken);

            if (transactionLineItem == null)
                return Result<TransactionLineItemDto>
                    .Failure("Transaction line item not found", 404);

            mapper.Map(request.TransactionLineItemDto, transactionLineItem);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<TransactionLineItemDto>
                    .Failure("Failed to update transaction line item", 400);

            var updatedTransactionLineItemDto = mapper
                .Map<TransactionLineItemDto>(transactionLineItem);

            return Result<TransactionLineItemDto>.Success(updatedTransactionLineItemDto);
        }
    }
}
