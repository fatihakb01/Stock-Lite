using Application.Core;
using Domain.Enums;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ConfirmedOrderAggregates.Features.Commands;

public class UpdateConfirmedOrderAggregates
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
        public string? PeriodType { get; set; }
    }

    public class Handler(
        IConfirmedOrderAggregateCalculation aggregateCalculation,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            Period? period = null;

            if (!string.IsNullOrWhiteSpace(request.PeriodType))
            {
                if (Enum.TryParse<Period>(request.PeriodType, true, out var parsed))
                    period = parsed;
                else
                    return Result<Unit>.Failure(
                        $"Invalid period type '{request.PeriodType}'. Allowed values: Daily, Weekly, Monthly.",
                        400
                    );
            }

            await aggregateCalculation.RunAsync(
                currentUser.CompanyId,
                request.ProductIds,
                period,
                ct
            );

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
