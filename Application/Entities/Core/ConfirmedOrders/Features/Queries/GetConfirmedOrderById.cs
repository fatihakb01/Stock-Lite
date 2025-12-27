using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Queries;

public class GetConfirmedOrderById
{
    public class Query : IRequest<Result<ConfirmedOrderDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ConfirmedOrderDto>>
    {
        public async Task<Result<ConfirmedOrderDto>> Handle(Query request, CancellationToken ct)
        {
            var orders = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                o => o.LineItems
            );

            var order = orders.SingleOrDefault();
            if (order == null)
                return Result<ConfirmedOrderDto>.Failure("Confirmed order not found", 404);

            var resultDto = mapper.Map<ConfirmedOrderDto>(order);

            return Result<ConfirmedOrderDto>.Success(resultDto);
        }
    }
}
