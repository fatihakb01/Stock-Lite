using Application.Core;
using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class CancelPurchaseOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> poRepository,
        IRepository<StockLevel> stockRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var pos = await poRepository.FindAsync(
                po => po.Id == request.Id && po.CompanyId == currentUser.CompanyId,
                ct,
                po => po.LineItems
            );
            var po = pos.SingleOrDefault();

            if (po == null)
                return Result<Unit>.Failure("Purchase order not found", 404);

            if (po.Status == PurchaseOrderStatus.Completed)
                return Result<Unit>.Failure("Completed completed orders cannot be cancelled", 404);

            if (po.Status is PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived)
            {
                foreach (var item in po.LineItems)
                {
                    var remaining = item.Quantity - item.ReceivedQuantity;
                    if (remaining <= 0)
                        continue;

                    var stocks = await stockRepository.FindAsync(
                        s => s.ProductId == item.ProductId,
                        ct
                    );

                    var stock = stocks.SingleOrDefault();
                    if (stock == null)
                        return Result<Unit>.Failure("Stock level not found", 404);

                    stock.QuantityOnOrder -= remaining;
                }
            }

            po.Status = PurchaseOrderStatus.Cancelled;

            await poRepository.SaveChangesAsync(ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
