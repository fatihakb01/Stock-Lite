using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Queries;

public class GetPurchaseOrdersByProductId
{
    public class Query : IRequest<Result<List<PurchaseOrderDto>>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<PurchaseOrder> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<PurchaseOrderDto>>>
    {
        public async Task<Result<List<PurchaseOrderDto>>> Handle(
            Query request,
            CancellationToken ct)
        {
            var purchaseOrders = await repository.FindAsync(
                po =>
                    po.CompanyId == currentUser.CompanyId &&
                    po.LineItems.Any(li => li.ProductId == request.ProductId),
                ct,
                po => po.LineItems.Where(li => li.ProductId == request.ProductId)
            );

            if (!purchaseOrders.Any())
                return Result<List<PurchaseOrderDto>>.Failure("No purchase orders found for this product", 404);

            var dto = mapper.Map<List<PurchaseOrderDto>>(purchaseOrders);

            return Result<List<PurchaseOrderDto>>.Success(dto);
        }
    }
}
