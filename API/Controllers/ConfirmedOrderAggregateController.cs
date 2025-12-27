using Application.Entities.Derived.ConfirmedOrderAggregates.Features.Commands;
using Application.Entities.Derived.ConfirmedOrderAggregates.Features.Queries;
using Domain.Entities.Derived;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ConfirmedOrderAggregateController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet("{periodType}/{productId}")]
    public async Task<ActionResult<ConfirmedOrderAggregate>> GetConfirmedOrderOutlook(
        string periodType,
        Guid productId
    )
    {
        return HandleResult(await Mediator.Send(new GetConfirmedOrderOutlook.Query
        {
            ProductId = productId,
            PeriodType = periodType
        }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{periodType?}")]
    public async Task<ActionResult<Unit>> UpdateConfirmedOrderAggregates(
        string? periodType,
        [FromBody] IEnumerable<Guid>? productIds
    )
    {
        return HandleResult(await Mediator.Send(new UpdateConfirmedOrderAggregates.Command
        {
            PeriodType = periodType,
            ProductIds = productIds
        }));
    }
}
