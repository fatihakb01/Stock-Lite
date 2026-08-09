using Application.Core;
using Application.Entities.Core.Transactions.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Transactions.Features.Queries;

public class GetTransactionsByProductId
{
    public class Query : IRequest<Result<List<TransactionDto>>>
    {
        public required Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<Transaction> repository,
        IBaseCacheService<Transaction> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<TransactionDto>>>
    {
        public async Task<Result<List<TransactionDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(request.ProductId, TransactionKey.Transaction);
            var cached = await cacheService.GetAsync<List<TransactionDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<TransactionDto>>.Success(cached);

            var transactions = await repository.FindAsync(
                t =>
                    t.CompanyId == currentUser.CompanyId &&
                    t.LineItems
                        .Any(li => li.ProductId == request.ProductId),
                ct,
                t => t.LineItems.Where(li => li.ProductId == request.ProductId)
            );

            transactions = transactions.OrderByDescending(t => t.TransactionDate);

            if (!transactions.Any())
                return Result<List<TransactionDto>>.Failure("No transactions found for this product", 404);

            var transactionDto = mapper.Map<List<TransactionDto>>(transactions);

            await cacheService.SetAsync(cacheKey, transactionDto, TimeSpan.FromMinutes(5), ct);

            return Result<List<TransactionDto>>.Success(transactionDto);
        }
    }
}
