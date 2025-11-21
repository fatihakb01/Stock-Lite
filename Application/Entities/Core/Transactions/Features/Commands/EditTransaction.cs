using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class EditTransaction
{
    public class Command : IRequest<Result<EditTransactionDto>>
    {
        public Guid Id { get; set; }
        public required EditTransactionDto TransactionDto { get; set; }
    }

    public class Handler(IRepository<Transaction> repository, IMapper mapper)
        : IRequestHandler<Command, Result<EditTransactionDto>>
    {
        public async Task<Result<EditTransactionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (transaction == null)
                return Result<EditTransactionDto>.Failure("Transaction not found", 404);

            mapper.Map(request.TransactionDto, transaction);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<EditTransactionDto>.Failure("Failed to update transaction", 400);

            var updatedTransactionDto = mapper.Map<EditTransactionDto>(transaction);

            return Result<EditTransactionDto>.Success(updatedTransactionDto);
        }
    }
}
