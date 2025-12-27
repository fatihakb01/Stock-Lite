using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Commands;

public class CreateConfirmedOrder
{
    public class Command : IRequest<Result<ConfirmedOrderDto>>
    {
        public required CreateConfirmedOrderDto ConfirmedOrderDto { get; set; }
    }

    public class Handler(
        IRepository<ConfirmedOrder> repository,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<ConfirmedOrderDto>>
    {
        public async Task<Result<ConfirmedOrderDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ConfirmedOrderDto;

            var orderExists = await repository.ExistsAsync(x =>
                x.OrderNumber == dto.OrderNumber && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (orderExists)
                return Result<ConfirmedOrderDto>.Failure("Order number must be unique", 400);

            var order = mapper.Map<ConfirmedOrder>(dto);
            order.CompanyId = currentUser.CompanyId;
            order.Status = ConfirmedOrderStatus.Confirmed;

            await repository.AddAsync(order, ct);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<ConfirmedOrderDto>.Failure("Failed to create confirmed order", 400);

            var resultDto = mapper.Map<ConfirmedOrderDto>(order);

            return Result<ConfirmedOrderDto>.Success(resultDto);
        }
    }
}
