using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Queries;

public class GetConfirmedOrders
{
    public class Query : IRequest<Result<List<ConfirmedOrderDto>>> {}

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ConfirmedOrderDto>>>
    {
        public async Task<Result<List<ConfirmedOrderDto>>> Handle(Query request, CancellationToken ct)
        {
            var orders = await repository.FindAsync(x =>
                x.CompanyId == currentUser.CompanyId,
                ct,
                o => o.LineItems
            );

            if (!orders.Any())
                return Result<List<ConfirmedOrderDto>>.Failure("No confirmed orders found", 404);

            var resultDto = mapper.Map<List<ConfirmedOrderDto>>(orders);

            return Result<List<ConfirmedOrderDto>>.Success(resultDto);
        }
    }
}
