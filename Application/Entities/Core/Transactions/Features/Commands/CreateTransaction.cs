using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class CreateTransaction
{
    public class Command : IRequest<Result<TransactionDto>>
    {
        public required CreateTransactionDto TransactionDto { get; set; }
    }

    public class Handler(IRepository<Transaction> repository, IMapper mapper)
        : IRequestHandler<Command, Result<TransactionDto>>
    {
        public async Task<Result<TransactionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = mapper.Map<Transaction>(request.TransactionDto);

            await repository.AddAsync(transaction, cancellationToken);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<TransactionDto>.Failure("Failed to create transaction", 400);

            var createdTransactionDto = mapper.Map<TransactionDto>(transaction);

            return Result<TransactionDto>.Success(createdTransactionDto);
        }
    }
}
