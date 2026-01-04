using Application.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.PurchaseForecasts.Features.Commands;

public class UpdatePurchaseForecasts
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
    }

    public class Handler(
        IPurchaseForecastRepository purchaseForecastRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            await purchaseForecastRepository.InsertPurchaseForecastsAsync(currentUser.CompanyId, request.ProductIds);
            await purchaseForecastRepository.UpdatePurchaseForecastsAsync(currentUser.CompanyId, request.ProductIds);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
