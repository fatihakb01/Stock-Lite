using Application.Entities.Core.PurchaseOrders.DTOs;
using Application.Entities.Core.PurchaseOrders.Features.Commands;
using Application.Entities.Core.PurchaseOrders.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PurchaseOrderController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetPurchaseOrderById(Guid id, [FromQuery] Guid companyId)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseOrderById.Query { Id = id, CompanyId = companyId }));
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetPurchaseOrders([FromQuery] Guid companyId)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseOrders.Query { CompanyId = companyId }));
    }   

    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder(
        [FromBody] CreatePurchaseOrderDto purchaseOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new CreatePurchaseOrder.Command { CreatePurchaseOrderDto = purchaseOrderDto }
            )
        );
    }

    [HttpPost("{id}/receive")]
    public async Task<ActionResult<PurchaseOrderDto>> ReceivePurchaseOrder(
        Guid id, [FromBody] ReceivePurchaseOrderDto purchaseOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new ReceivePurchaseOrder.Command { PurchaseOrderId = id, ReceivePurchaseOrderDto = purchaseOrderDto }
            )
        );
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<PurchaseOrderDto>> SubmitPurchaseOrder(
        Guid id, [FromQuery] Guid companyId)
    {
        return HandleResult(
            await Mediator.Send(
                new SubmitPurchaseOrder.Command { Id = id, CompanyId = companyId }
            )
        );
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<PurchaseOrderDto>> CancelPurchaseOrder(
        Guid id, [FromQuery] Guid companyId)
    {
        return HandleResult(
            await Mediator.Send(
                new CancelPurchaseOrder.Command { Id = id, CompanyId = companyId }
            )
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PurchaseOrderDto>> UpdatePurchaseOrder(
        Guid id, [FromBody] UpdatePurchaseOrderDto purchaseOrderDto)
    {
        return HandleResult(
            await Mediator.Send(
                new UpdatePurchaseOrder.Command {PurchaseOrderId = id, UpdatePurchaseOrderDto = purchaseOrderDto}
            )
        );
    }
}
