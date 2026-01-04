using Application.Entities.Derived.PurchaseForecasts.DTOs;
using Application.Entities.Derived.PurchaseForecasts.Features.Commands;
using Application.Entities.Derived.PurchaseForecasts.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class PurchaseForecastController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{productId}")]
    public async Task<ActionResult<List<PurchaseForecastDto>>> GetPurchaseForecast(Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseForecast.Query
        {
            ProductId = productId
        }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Unit>> UpdatePurchaseForecasts([FromBody] IEnumerable<Guid>? productIds)
    {
        return HandleResult(await Mediator.Send(new UpdatePurchaseForecasts.Command
        {
            ProductIds = productIds
        }));
    }
}
