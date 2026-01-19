using Application.Core;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Services.Scheduler.Calculations;
using MediatR;

namespace Application.Entities.Derived.Forecasts.Features.Commands;

public class GenerateForecasts
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
        public string? PeriodType { get; set; }
    }

    public class Handler(
        IForecastCalculation forecastCalculation,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            // Parse period type
            Period? period = null;
            if (!string.IsNullOrWhiteSpace(request.PeriodType))
            {
                if (Enum.TryParse<Period>(request.PeriodType, true, out var parsed))
                    period = parsed;
                else
                    return Result<Unit>.Failure($"Invalid period type '{request.PeriodType}'. Allowed values: Daily, Weekly, Monthly.", 400);
            }

            // Run aggregation
            await forecastCalculation.RunAsync(currentUser.CompanyId, request.ProductIds, period, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }

}
