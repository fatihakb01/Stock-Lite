using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Queries;

public class GetPurchaseOrders
{
    public class Query : IRequest<Result<List<PurchaseOrderDto>>> { }

    public class Handler(
        IRepository<PurchaseOrder> repository,
        IBaseCacheService<Transaction> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<PurchaseOrderDto>>>
    {
        public async Task<Result<List<PurchaseOrderDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.CompanyId, PurchaseOrderKey.PurchaseOrders);
            var cached = await cacheService.GetAsync<List<PurchaseOrderDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<PurchaseOrderDto>>.Success(cached);

            var pos = await repository.FindAsync(x => 
                x.CompanyId == currentUser.CompanyId, 
                ct,
                po => po.LineItems
            );

            if (!pos.Any())
                return Result<List<PurchaseOrderDto>>.Failure("Purchase Order not found", 404);

            var resultDto = mapper.Map<List<PurchaseOrderDto>>(pos);

            return Result<List<PurchaseOrderDto>>.Success(resultDto);
        }
    }
}
