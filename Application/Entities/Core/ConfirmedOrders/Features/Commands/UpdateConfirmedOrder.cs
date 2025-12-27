using Application.Core;
using Application.Entities.Core.ConfirmedOrders.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.ConfirmedOrders.Features.Commands;

public class UpdateConfirmedOrder
{
    public class Command : IRequest<Result<ConfirmedOrderDto>>
    {
        public Guid Id { get; set; }
        public required UpdateConfirmedOrderDto ConfirmedOrderDto { get; set; }
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

            var orders = await repository.FindAsync(x =>
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                o => o.LineItems
            );

            var order = orders.SingleOrDefault();
            if (order == null)
                return Result<ConfirmedOrderDto>.Failure("Confirmed order not found", 404);

            if (order.Status == ConfirmedOrderStatus.Fulfilled)
                return Result<ConfirmedOrderDto>.Failure("Cannot update a fulfilled order", 400);

            var orderExists = await repository.ExistsAsync(x =>
                x.Id != request.Id &&
                x.OrderNumber == dto.OrderNumber && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (orderExists)
                return Result<ConfirmedOrderDto>.Failure("Order number must be unique", 400);

            mapper.Map(dto, order);

            var result = await repository.SaveChangesAsync(ct);
            if (!result)
                return Result<ConfirmedOrderDto>.Failure("Failed to update confirmed order", 400);

            var resultDto = mapper.Map<ConfirmedOrderDto>(order);

            return Result<ConfirmedOrderDto>.Success(resultDto);
        }
    }
}
