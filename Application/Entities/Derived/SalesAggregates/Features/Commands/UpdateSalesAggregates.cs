using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.SalesAggregates.Features.Commands;

public class UpdateSalesAggregates
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
        public string? PeriodType { get; set; }
    }

    public class Handler(
        ISalesAggregateRepository aggregateRepo,
        ICurrentUserService currentUser
    )
        : IRequestHandler<Command, Result<Unit>>
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
            await aggregateRepo.InsertSalesAggregatesAsync(currentUser.CompanyId, request.ProductIds, period);
            await aggregateRepo.UpdateSalesAggregatesAsync(currentUser.CompanyId, request.ProductIds, period);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
