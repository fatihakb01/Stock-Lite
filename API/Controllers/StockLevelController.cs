using Application.Entities.Derived.StockLevels.DTOs;
using Application.Entities.Derived.StockLevels.Features.Commands;
using Application.Entities.Derived.StockLevels.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class StockLevelController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{productId}")]
    public async Task<ActionResult<StockLevelDto>> GetStockLevel(Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetStockLevel.Query
        {
            ProductId = productId
        }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Unit>> UpdateStockLevels([FromBody] IEnumerable<Guid>? productIds)
    {
        return HandleResult(await Mediator.Send(new UpdateStockLevels.Command
        {
            ProductIds = productIds
        }));
    }
}
