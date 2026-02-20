using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Commands;

public class CreateTransaction
{
    public class Command : IRequest<Result<TransactionDto>>
    {
        public required CreateTransactionDto TransactionDto { get; set; }
    }

    public class Handler(
        IRepository<Transaction> transactionRepo, 
        IProductRepository productRepo,
        IMapper mapper,
        ICurrentUserService currentUser
    )
        : IRequestHandler<Command, Result<TransactionDto>>
    {
        public async Task<Result<TransactionDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.TransactionDto;

            var productIds = dto.LineItems
                .Select(li => li.ProductId)
                .Distinct()
                .ToList();

            var validProductCount = await productRepo.CountAsync(
                p => productIds.Contains(p.Id) && 
                p.CompanyId == currentUser.CompanyId,
                ct
            );

            if (validProductCount != productIds.Count)
                return Result<TransactionDto>
                    .Failure("One or more products do not belong to your company", 400);

            var transactionIdExist = await transactionRepo.ExistsAsync(x =>
                x.TransactionId == dto.TransactionId && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (transactionIdExist)
                return Result<TransactionDto>.Failure("Transaction Id must be unique", 400);

            var transaction = mapper.Map<Transaction>(dto);
            transaction.CompanyId = currentUser.CompanyId;
            transaction.TotalAmount = dto.LineItems.Sum(x => x.Quantity * x.UnitPrice);

            await transactionRepo.AddAsync(transaction, ct);

            var result = await transactionRepo.SaveChangesAsync(ct);

            if (!result)
                return Result<TransactionDto>.Failure("Failed to create transaction", 400);

            var createdTransactionDto = mapper.Map<TransactionDto>(transaction);

            return Result<TransactionDto>.Success(createdTransactionDto);
        }
    }
}
