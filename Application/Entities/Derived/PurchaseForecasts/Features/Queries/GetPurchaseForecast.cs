using Application.Core;
using Application.Entities.Derived.PurchaseForecasts.DTOs;
using AutoMapper;
using Domain.Entities.Derived;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.PurchaseForecasts.Features.Queries;

public class GetPurchaseForecast
{
    public class Query : IRequest<Result<List<PurchaseForecastDto>>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IPurchaseForecastRepository purchaseForecastRepo,
        IProductRepository productRepo,
        IBaseCacheService<PurchaseForecast> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<PurchaseForecastDto>>>
    {
        public async Task<Result<List<PurchaseForecastDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(request.ProductId, PurchaseForecastKey.PurchaseForecast);
            var cached = await cacheService.GetAsync<List<PurchaseForecastDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<PurchaseForecastDto>>.Success(cached);

            var products = await productRepo.ExistsAsync(x =>
                x.Id == request.ProductId && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!products)
                return Result<List<PurchaseForecastDto>>.Failure($"Product not found for this company", 404);

            var pfc = await purchaseForecastRepo.GetByProductAsync(request.ProductId);

            if (pfc == null)
                return Result<List<PurchaseForecastDto>>.Failure($"No stock level available", 404);

            var pfcDto = mapper.Map<List<PurchaseForecastDto>>(pfc);
            await cacheService.SetAsync(cacheKey, pfcDto, TimeSpan.FromMinutes(5), ct);
            return Result<List<PurchaseForecastDto>>.Success(pfcDto);
        }
    }
}
