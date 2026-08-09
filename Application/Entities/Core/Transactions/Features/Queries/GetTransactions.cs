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

public class GetTransactions
{
    public class Query : IRequest<Result<List<TransactionDto>>> { }

    public class Handler(
        IRepository<Transaction> repository,
        IBaseCacheService<Transaction> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<TransactionDto>>>
    {
        public async Task<Result<List<TransactionDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.CompanyId, TransactionKey.Transactions);
            var cached = await cacheService.GetAsync<List<TransactionDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<TransactionDto>>.Success(cached);

            var transactions = await repository.FindAsync(x =>
                x.CompanyId == currentUser.CompanyId,
                ct,
                t => t.LineItems
            );

            if (!transactions.Any())
                return Result<List<TransactionDto>>.Failure("Transactions not found", 404);

            var transactionDtos = mapper.Map<List<TransactionDto>>(transactions);

            await cacheService.SetAsync(cacheKey, transactionDtos, TimeSpan.FromMinutes(5), ct);

            return Result<List<TransactionDto>>.Success(transactionDtos);
        }
    }
}
