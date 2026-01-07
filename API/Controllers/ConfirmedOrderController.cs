using Application.Entities.Core.ConfirmedOrders.DTOs;
using Application.Entities.Core.ConfirmedOrders.Features.Commands;
using Application.Entities.Core.ConfirmedOrders.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ConfirmedOrderController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("by-product/{productId}")]
    public async Task<ActionResult<List<ConfirmedOrderDto>>> GetConfirmedOrdersByProductId(Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetConfirmedOrdersByProductId.Query { ProductId = productId }));
    }

    [HttpGet]
    public async Task<ActionResult<List<ConfirmedOrderDto>>> GetConfirmedOrders()
    {
        return HandleResult(await Mediator.Send(new GetConfirmedOrders.Query()));
    }   

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ConfirmedOrderDto>> CreateConfirmedOrder(
        [FromBody] CreateConfirmedOrderDto ConfirmedOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new CreateConfirmedOrder.Command { ConfirmedOrderDto = ConfirmedOrderDto }
            )
        );
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{id}/receive")]
    public async Task<ActionResult<ConfirmedOrderDto>> ReceiveConfirmedOrder(
        Guid id, [FromBody] ReceiveConfirmedOrderDto ConfirmedOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new ReceiveConfirmedOrder.Command { Id = id, ReceiveDto = ConfirmedOrderDto }
            )
        );
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ConfirmedOrderDto>> CancelConfirmedOrder(Guid id)
    {
        return HandleResult(
            await Mediator.Send(
                new CancelConfirmedOrder.Command { Id = id }
            )
        );
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ConfirmedOrderDto>> UpdateConfirmedOrder(
        Guid id, [FromBody] UpdateConfirmedOrderDto ConfirmedOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new UpdateConfirmedOrder.Command {Id = id, ConfirmedOrderDto = ConfirmedOrderDto}
            )
        );
    }
}
