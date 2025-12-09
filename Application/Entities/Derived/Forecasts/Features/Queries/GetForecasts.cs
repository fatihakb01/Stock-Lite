using Application.Core;
using Application.Entities.Derived.Forecasts.DTOs;
using AutoMapper;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.Forecasts.Features.Queries;

public class GetForecasts
{
    public class Query : IRequest<Result<List<ForecastDto>>>
    {
        public Guid ProductId { get; set; }
        public string PeriodType { get; set; } = "Daily";
    }

    public class Handler(IForecastRepository repository, IMapper mapper)
    : IRequestHandler<Query, Result<List<ForecastDto>>>
    {
        public async Task<Result<List<ForecastDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<List<ForecastDto>>.Failure($"Invalid period type: {request.PeriodType}", 400);

            var forecast = await repository.GetForecastByProductAsync(request.ProductId, parsedPeriod);

            if (forecast == null || !forecast.Any())
                return Result<List<ForecastDto>>.Failure("No history available", 404);

            var forecastDto = mapper.Map<List<ForecastDto>>(forecast);

            return Result<List<ForecastDto>>.Success(forecastDto);
        }
    }

}
