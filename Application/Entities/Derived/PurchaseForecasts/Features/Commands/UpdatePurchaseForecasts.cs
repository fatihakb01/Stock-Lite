using Application.Core;
using Domain.Interfaces;
using Domain.Services.Scheduler.Calculations;
using MediatR;

namespace Application.Entities.Derived.PurchaseForecasts.Features.Commands;

public class UpdatePurchaseForecasts
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
    }

    public class Handler(
        IPurchaseForecastCalculation purchaseForecastCalculation,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            await purchaseForecastCalculation.RunAsync(currentUser.CompanyId, request.ProductIds, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
