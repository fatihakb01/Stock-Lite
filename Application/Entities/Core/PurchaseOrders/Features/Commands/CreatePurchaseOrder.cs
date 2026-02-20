using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using MediatR;
using Domain.Enums;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;

namespace Application.Entities.Core.PurchaseOrders.Features.Commands;

public class CreatePurchaseOrder
{
    public class Command : IRequest<Result<PurchaseOrderDto>>
    {
        public required CreatePurchaseOrderDto CreatePurchaseOrderDto { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<PurchaseOrderDto>>
    {
        public async Task<Result<PurchaseOrderDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CreatePurchaseOrderDto;

            var orderNumberExist = await repository.ExistsAsync(x =>
                x.OrderNumber == dto.OrderNumber && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (orderNumberExist)
                return Result<PurchaseOrderDto>.Failure("Order number must be unique", 400);

            var purchaseOrder = mapper.Map<PurchaseOrder>(dto);
            purchaseOrder.Status = PurchaseOrderStatus.Draft;
            purchaseOrder.CompanyId = currentUser.CompanyId;

            await repository.AddAsync(purchaseOrder, ct);
            await repository.SaveChangesAsync(ct);

            var resultDto = mapper.Map<PurchaseOrderDto>(purchaseOrder);

            return Result<PurchaseOrderDto>.Success(resultDto);
        }
    }
}
