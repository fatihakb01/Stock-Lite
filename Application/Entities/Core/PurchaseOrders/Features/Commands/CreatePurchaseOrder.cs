using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Domain.Enums;
using Domain.Entities.Core;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class CreatePurchaseOrder
{
    public class Command : IRequest<Result<PurchaseOrderDto>>
    {
        public required CreatePurchaseOrderDto CreatePurchaseOrderDto { get; set; }
    }

    public class Handler(IRepository<PurchaseOrder> repository, IMapper mapper)
        : IRequestHandler<Command, Result<PurchaseOrderDto>>
    {
        public async Task<Result<PurchaseOrderDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CreatePurchaseOrderDto;

            var purchaseOrder = mapper.Map<PurchaseOrder>(dto);
            purchaseOrder.Status = PurchaseOrderStatus.Draft;

            await repository.AddAsync(purchaseOrder, ct);
            await repository.SaveChangesAsync(ct);

            var resultDto = mapper.Map<PurchaseOrderDto>(purchaseOrder);

            return Result<PurchaseOrderDto>.Success(resultDto);
        }
    }
}
