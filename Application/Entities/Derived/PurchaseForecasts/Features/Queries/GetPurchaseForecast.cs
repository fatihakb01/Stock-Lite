using Application.Core;
using Application.Entities.Derived.PurchaseForecasts.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
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
        IRepository<Product> productRepo,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<PurchaseForecastDto>>>
    {
        public async Task<Result<List<PurchaseForecastDto>>> Handle(Query request, CancellationToken ct)
        {
            var products = await productRepo.FindAsync(x =>
                x.Id == request.ProductId && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!products.Any())
                return Result<List<PurchaseForecastDto>>.Failure($"Product not found for this company", 404);

            var pfc = await purchaseForecastRepo.GetByProductAsync(request.ProductId);

            if (pfc == null)
                return Result<List<PurchaseForecastDto>>.Failure($"No stock level available", 404);

            var pfcDto = mapper.Map<List<PurchaseForecastDto>>(pfc);

            return Result<List<PurchaseForecastDto>>.Success(pfcDto);
        }
    }
}
