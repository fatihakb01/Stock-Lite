using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class UpdatePurchaseOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public required Guid PurchaseOrderId { get; set; }
        public required UpdatePurchaseOrderDto UpdatePurchaseOrderDto { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> poRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.UpdatePurchaseOrderDto;

            var orderNumberExist = await poRepository.ExistsAsync(x =>
                x.OrderNumber == dto.OrderNumber && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (orderNumberExist)
                return Result<Unit>.Failure("Order number must be unique", 400);

            var pos = await poRepository.FindAsync(
                po => po.Id == request.PurchaseOrderId && po.CompanyId == currentUser.CompanyId,
                ct,
                po => po.LineItems
            );
            var po = pos.SingleOrDefault();
            
            if (po == null)
                return Result<Unit>.Failure("Purchase order not found", 404);

            if (po.Status != PurchaseOrderStatus.Draft)
                return Result<Unit>.Failure("Only draft purchase orders can be updated", 400);

            if (dto.ExpectedDeliveryDate.HasValue)
                po.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;

            foreach (var itemDto in dto.LineItems)
            {
                var lineItem = po.LineItems.SingleOrDefault(li => li.ProductId == itemDto.ProductId);
                if (lineItem == null)
                    return Result<Unit>.Failure("Line item not found", 404);

                lineItem.Quantity = itemDto.Quantity;
                lineItem.UnitCost = itemDto.UnitCost;
            }

            await poRepository.SaveChangesAsync(ct);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
