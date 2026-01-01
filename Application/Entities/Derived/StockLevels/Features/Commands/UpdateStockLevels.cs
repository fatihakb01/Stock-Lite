using Application.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.StockLevels.Features.Commands;

public class UpdateStockLevels
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
    }

    public class Handler(
        IStockLevelRepository stockLevelRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            await stockLevelRepository.InsertStockLevelsAsync(currentUser.CompanyId, request.ProductIds);
            await stockLevelRepository.UpdateStockLevelsAsync(currentUser.CompanyId, request.ProductIds);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
