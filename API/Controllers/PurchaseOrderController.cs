using Application.Entities.Core.PurchaseOrders.DTOs;
using Application.Entities.Core.PurchaseOrders.Features.Commands;
using Application.Entities.Core.PurchaseOrders.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class PurchaseOrderController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("by-product/{productId}")]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetPurchaseOrdersByProductId(Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseOrdersByProductId.Query { ProductId = productId }));
    }

    //[HttpGet]
    //public async Task<ActionResult<List<PurchaseOrderDto>>> GetPurchaseOrders()
    //{
    //    return HandleResult(await Mediator.Send(new GetPurchaseOrders.Query { }));
    //}   

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost]
    //public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder(
    //    [FromBody] CreatePurchaseOrderDto purchaseOrderDto)
    //{
    //    return HandleResult(
    //        await Mediator.Send(
    //            new CreatePurchaseOrder.Command { CreatePurchaseOrderDto = purchaseOrderDto }
    //        )
    //    );
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost("{id}/receive")]
    //public async Task<ActionResult<PurchaseOrderDto>> ReceivePurchaseOrder(
    //    Guid id, [FromBody] ReceivePurchaseOrderDto purchaseOrderDto)
    //{
    //    return HandleResult(
    //        await Mediator.Send(
    //            new ReceivePurchaseOrder.Command { PurchaseOrderId = id, ReceivePurchaseOrderDto = purchaseOrderDto }
    //        )
    //    );
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost("{id}/submit")]
    //public async Task<ActionResult<PurchaseOrderDto>> SubmitPurchaseOrder(Guid id)
    //{
    //    return HandleResult(
    //        await Mediator.Send(
    //            new SubmitPurchaseOrder.Command { Id = id }
    //        )
    //    );
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost("{id}/cancel")]
    //public async Task<ActionResult<PurchaseOrderDto>> CancelPurchaseOrder(Guid id)
    //{
    //    return HandleResult(
    //        await Mediator.Send(
    //            new CancelPurchaseOrder.Command { Id = id }
    //        )
    //    );
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPut("{id}")]
    //public async Task<ActionResult<PurchaseOrderDto>> UpdatePurchaseOrder(
    //    Guid id, [FromBody] UpdatePurchaseOrderDto purchaseOrderDto)
    //{
    //    return HandleResult(
    //        await Mediator.Send(
    //            new UpdatePurchaseOrder.Command {PurchaseOrderId = id, UpdatePurchaseOrderDto = purchaseOrderDto}
    //        )
    //    );
    //}
}
