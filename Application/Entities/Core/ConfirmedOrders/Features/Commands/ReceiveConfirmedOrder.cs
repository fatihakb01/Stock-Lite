using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Commands;

public class ReceiveConfirmedOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
        public required ReceiveConfirmedOrderDto ReceiveDto { get; set; }
    }

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ReceiveDto;

            var orders = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                o => o.LineItems
            );

            var order = orders.SingleOrDefault();
            if (order == null)
                return Result<Unit>.Failure("Confirmed order not found", 404);

            foreach (var itemDto in dto.LineItems)
            {
                var lineItem = order.LineItems
                    .SingleOrDefault(li => li.ProductId == itemDto.ProductId);

                if (lineItem == null)
                    return Result<Unit>.Failure("Invalid product in confirmed order", 400);

                lineItem.SoldQuantity += itemDto.QuantitySold;
            }

            order.Status = order.LineItems.All(li => li.SoldQuantity >= li.Quantity)
                ? ConfirmedOrderStatus.Fulfilled
                : ConfirmedOrderStatus.PartiallyFulfilled;

            if (order.Status == ConfirmedOrderStatus.Fulfilled)
                order.ReceivedDate = DateTime.UtcNow;

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<Unit>.Failure("Failed to receive confirmed order", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
