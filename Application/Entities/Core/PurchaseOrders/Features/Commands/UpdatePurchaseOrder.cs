using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class UpdatePurchaseOrder
{
    public class Command : IRequest<Result<PurchaseOrderDto>>
    {
        public required Guid PurchaseOrderId { get; set; }
        public required UpdatePurchaseOrderDto UpdatePurchaseOrderDto { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> poRepository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<PurchaseOrderDto>>
    {
        public async Task<Result<PurchaseOrderDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.UpdatePurchaseOrderDto;

            var orderNumberExist = await poRepository.ExistsAsync(x =>
                x.Id != request.PurchaseOrderId &&
                x.OrderNumber == dto.OrderNumber && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (orderNumberExist)
                return Result<PurchaseOrderDto>.Failure("Order number must be unique", 400);

            var pos = await poRepository.FindAsync(
                po => po.Id == request.PurchaseOrderId && po.CompanyId == currentUser.CompanyId,
                ct,
                po => po.LineItems
            );
            var po = pos.SingleOrDefault();
            
            if (po == null)
                return Result<PurchaseOrderDto>.Failure("Purchase order not found", 404);

            if (po.Status != PurchaseOrderStatus.Draft)
                return Result<PurchaseOrderDto>.Failure("Only draft purchase orders can be updated", 400);

            foreach (var itemDto in dto.LineItems)
            {
                var lineItem = po.LineItems.SingleOrDefault(li => li.ProductId == itemDto.ProductId);
                if (lineItem == null)
                    return Result<PurchaseOrderDto>.Failure("Line item not found", 404);

                lineItem.Quantity = itemDto.Quantity;
                lineItem.UnitCost = itemDto.UnitCost;
            }

            mapper.Map(dto, po);

            await poRepository.SaveChangesAsync(ct);

            var resultDto = mapper.Map<PurchaseOrderDto>(po);

            return Result<PurchaseOrderDto>.Success(resultDto);
        }
    }
}
