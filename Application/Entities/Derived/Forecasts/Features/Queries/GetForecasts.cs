using Application.Core;
using Application.Entities.Derived.Forecasts.DTOs;
using AutoMapper;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.Forecasts.Features.Queries;

public class GetForecasts
{
    public class Query : IRequest<Result<List<ForecastDto>>>
    {
        public Guid ProductId { get; set; }
        public string PeriodType { get; set; } = "Daily";
    }

    public class Handler(
        IForecastRepository forecastRepo, 
        IProductRepository productRepo,
        IBaseCacheService<Forecast> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ForecastDto>>>
    {
        public async Task<Result<List<ForecastDto>>> Handle(Query request, CancellationToken ct)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<List<ForecastDto>>.Failure($"Invalid period type: {request.PeriodType}", 400);

            var cacheKey = cacheService.GetKey(request.ProductId, parsedPeriod);
            var cached = await cacheService.GetAsync<List<ForecastDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<ForecastDto>>.Success(cached);

            var products = await productRepo.ExistsAsync(x =>
                x.Id == request.ProductId && x.CompanyId == currentUser.CompanyId,
                ct
            );
            if (!products)
                return Result<List<ForecastDto>>.Failure($"Product not found for this company", 404);

            var forecast = await forecastRepo.GetForecastByProductAsync(request.ProductId, parsedPeriod);
            if (!forecast.Any())
                return Result<List<ForecastDto>>.Failure("No history available", 404);

            var forecastDto = mapper.Map<List<ForecastDto>>(forecast);
            await cacheService.SetAsync(cacheKey, forecastDto, TimeSpan.FromMinutes(5), ct);
            return Result<List<ForecastDto>>.Success(forecastDto);
        }
    }

}
