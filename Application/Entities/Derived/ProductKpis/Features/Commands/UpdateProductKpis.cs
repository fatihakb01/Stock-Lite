using Application.Core;
using Domain.Interfaces.Services.Scheduler.Calculations;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ProductKpis.Features.Commands;

public class UpdateProductKpis
{
    public class Command : IRequest<Result<Unit>> { }

    public class Handler(
        IProductKpiCalculation repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            await repository.RunAsync(currentUser.CompanyId, ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
