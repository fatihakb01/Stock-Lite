using Application.Core;
using Application.Entities.Core.PurchaseOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.PurchaseOrders.Features.Queries;

public class GetPurchaseOrderById
{
    public class Query : IRequest<Result<PurchaseOrderDto>>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }

    public class Handler(IRepository<PurchaseOrder> repository, IMapper mapper)
        : IRequestHandler<Query, Result<PurchaseOrderDto>>
    {
        public async Task<Result<PurchaseOrderDto>> Handle(Query request, CancellationToken ct)
        {
            var pos = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == request.CompanyId,
                ct,
                po => po.LineItems
            );
            var po = pos.SingleOrDefault();

            if (po == null)
                return Result<PurchaseOrderDto>.Failure("Purchase Order not found", 404);

            var resultDto = mapper.Map<PurchaseOrderDto>(po);

            return Result<PurchaseOrderDto>.Success(resultDto);
        }
    }
}
