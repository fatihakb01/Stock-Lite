using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Queries;

public class GetConfirmedOrders
{
    public class Query : IRequest<Result<List<ConfirmedOrderDto>>> {}

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        IBaseCacheService<ConfirmedOrder> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ConfirmedOrderDto>>>
    {
        public async Task<Result<List<ConfirmedOrderDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.CompanyId, ConfirmedOrderKey.ConfirmedOrders);
            var cached = await cacheService.GetAsync<List<ConfirmedOrderDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<ConfirmedOrderDto>>.Success(cached);

            var orders = await repository.FindAsync(x =>
                x.CompanyId == currentUser.CompanyId,
                ct,
                o => o.LineItems
            );

            if (!orders.Any())
                return Result<List<ConfirmedOrderDto>>.Failure("No confirmed orders found", 404);

            var resultDto = mapper.Map<List<ConfirmedOrderDto>>(orders);

            await cacheService.SetAsync(cacheKey, resultDto, TimeSpan.FromMinutes(5), ct);

            return Result<List<ConfirmedOrderDto>>.Success(resultDto);
        }
    }
}
