using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.Forecasts.Features.Commands;

public class GenerateForecasts
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
        public string? PeriodType { get; set; }
        public Guid CompanyId { get; set; }
    }

    public class Handler(
        IRepository<Product> productRepo, 
        IForecastRepository forecastRepo
    )
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate company existence
            var companyExists = await productRepo.ExistsAsync(x => x.CompanyId == request.CompanyId, cancellationToken);
            if (!companyExists)
                return Result<Unit>.Failure("Company not found", 404);

            // Parse period type
            Period? period = null;
            if (!string.IsNullOrWhiteSpace(request.PeriodType))
            {
                if (Enum.TryParse<Period>(request.PeriodType, true, out var parsed))
                    period = parsed;
                else
                    return Result<Unit>.Failure($"Invalid period type '{request.PeriodType}'. Allowed values: Daily, Weekly, Monthly.", 400);
            }

            // Retrieve product IDs
            IEnumerable<Guid>? productIds = request.ProductIds;
            if (productIds == null || !productIds.Any())
            {
                productIds = await productRepo.FindSelectAsync(
                    p => p.CompanyId == request.CompanyId,
                    p => p.Id,
                    cancellationToken
                );
            }

            await forecastRepo.InsertForecastsAsync(request.CompanyId, productIds, period);
            await forecastRepo.UpdateForecastsAsync(request.CompanyId, productIds, period);

            return Result<Unit>.Success(Unit.Value);
        }
    }

}
