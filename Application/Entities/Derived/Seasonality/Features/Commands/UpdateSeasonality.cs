using Application.Core;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.Seasonality.Features.Commands;

public class UpdateSeasonality
{
    public class Command : IRequest<Result<Unit>>
    {
        public IEnumerable<Guid>? ProductIds { get; set; }
    }

    public class Handler(
        ISeasonalityCalculation repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            await repository.RunAsync(currentUser.CompanyId, request.ProductIds, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
