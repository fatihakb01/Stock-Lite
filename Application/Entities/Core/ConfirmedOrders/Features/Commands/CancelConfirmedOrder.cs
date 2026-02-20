using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Commands;

public class CancelConfirmedOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var order = await repository.SingleOrDefaultAsync(
                co => co.Id == request.Id && co.CompanyId == currentUser.CompanyId,
                ct,
                co => co.LineItems
            );

            if (order == null)
                return Result<Unit>.Failure("Confirmed order not found", 404);

            if (order.Status == ConfirmedOrderStatus.Fulfilled)
                return Result<Unit>.Failure("Fulfilled confirmed orders cannot be cancelled", 400);

            if (order.Status == ConfirmedOrderStatus.PartiallyFulfilled)
            {
                foreach (var lineItem in order.LineItems)
                {
                    lineItem.SoldQuantity = 0;
                }
            }

            order.Status = ConfirmedOrderStatus.Cancelled;

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<Unit>.Failure("Failed to cancel confirmed order", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
