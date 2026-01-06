using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Queries;

public class GetConfirmedOrdersByProductId
{
    public class Query : IRequest<Result<List<ConfirmedOrderDto>>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ConfirmedOrderDto>>>
    {
        public async Task<Result<List<ConfirmedOrderDto>>> Handle(Query request, CancellationToken ct)
        {
            var confirmedOrders = await repository.FindAsync(
                co =>
                    co.CompanyId == currentUser.CompanyId &&
                    co.LineItems.Any(li => li.ProductId == request.ProductId),
                ct,
                co => co.LineItems.Where(li => li.ProductId == request.ProductId)
            );

            if (!confirmedOrders.Any())
                return Result<List<ConfirmedOrderDto>>.Failure("No confirmed orders found for this product", 404);

            var resultDto = mapper.Map<List<ConfirmedOrderDto>>(confirmedOrders);

            return Result<List<ConfirmedOrderDto>>.Success(resultDto);
        }
    }
}
