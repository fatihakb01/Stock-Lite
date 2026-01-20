using Application.Core;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.StockLevels.Features.Commands;

public class UpdateStockLevels
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
    }

    public class Handler(
        IStockLevelCalculation stockLevelCalculation,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            await stockLevelCalculation.RunAsync(currentUser.CompanyId, request.ProductIds, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
