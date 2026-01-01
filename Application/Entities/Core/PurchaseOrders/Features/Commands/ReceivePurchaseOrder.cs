using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class ReceivePurchaseOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid PurchaseOrderId { get; set; }
        public required ReceivePurchaseOrderDto ReceivePurchaseOrderDto { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> poRepository,
        IRepository<StockLevel> stockRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ReceivePurchaseOrderDto;

            var pos = await poRepository.FindAsync(
                po => po.Id == request.PurchaseOrderId
                   && po.CompanyId == currentUser.CompanyId,
                ct,
                po => po.LineItems
            );

            var po = pos.SingleOrDefault();
            if (po == null)
                return Result<Unit>.Failure("Purchase order not found", 404);

            if (po.Status is not (PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived))
                return Result<Unit>.Failure("Purchase order cannot be received in its current state", 400);

            foreach (var itemDto in dto.lineItems)
            {
                var lineItem = po.LineItems
                    .SingleOrDefault(li => li.ProductId == itemDto.ProductId);

                if (lineItem == null)
                    return Result<Unit>.Failure("Invalid line item", 404);

                var remaining = lineItem.Quantity - lineItem.ReceivedQuantity;
                if (itemDto.QuantityReceived > remaining)
                    return Result<Unit>.Failure("Received quantity exceeds ordered quantity", 400);

                var stocks = await stockRepository.FindAsync(
                    s => s.ProductId == lineItem.ProductId,
                    ct
                );

                var stock = stocks.SingleOrDefault();
                if (stock == null)
                    return Result<Unit>.Failure("Stock level not found", 404);

                // Inventory update
                stock.QuantityOnHand += itemDto.QuantityReceived;
                stock.QuantityOnOrder -= itemDto.QuantityReceived;

                lineItem.ReceivedQuantity += itemDto.QuantityReceived;
            }

            po.Status = po.LineItems.All(li => li.ReceivedQuantity == li.Quantity)
                ? PurchaseOrderStatus.Completed
                : PurchaseOrderStatus.PartiallyReceived;

            po.ReceivedDate ??= DateTime.UtcNow;

            await poRepository.SaveChangesAsync(ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
