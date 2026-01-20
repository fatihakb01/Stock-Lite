using Application.Core;
using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class SubmitPurchaseOrder
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
                po => po.Id == request.Id
                   && po.CompanyId == currentUser.CompanyId,
                ct,
                po => po.LineItems
            );

            var po = pos.SingleOrDefault();
            if (po == null)
                return Result<Unit>.Failure("Purchase order not found", 404);

            if (po.Status != PurchaseOrderStatus.Draft)
                return Result<Unit>.Failure("Only draft orders can be submitted", 404);

            foreach (var item in po.LineItems)
            {
                var stocks = await stockRepository.FindAsync(
                    s => s.ProductId == item.ProductId,
                    ct
                );

                var stock = stocks.SingleOrDefault();
                if (stock == null)
                    return Result<Unit>.Failure("Stock level not found", 404);

                stock.QuantityOnOrder += item.Quantity;
            }

            po.Status = PurchaseOrderStatus.Submitted;

            await poRepository.SaveChangesAsync(ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
