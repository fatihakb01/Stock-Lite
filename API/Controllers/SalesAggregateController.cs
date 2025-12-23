using Application.Entities.Derived.SalesAggregates.Features.Commands;
using Application.Entities.Derived.SalesAggregates.Features.Queries;
using Domain.Entities.Derived;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class SalesAggregateController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{periodType}/{productId}")]
    public async Task<ActionResult<SalesAggregate>> GetSalesTrend(string periodType, Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetSalesTrend.Query
        {
            ProductId = productId,
            PeriodType = periodType
        }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{periodType?}")]
    public async Task<ActionResult<SalesAggregate>> UpdateSalesAggregates
        (string? periodType, [FromBody] IEnumerable<Guid>? productIds)
    {
        return HandleResult(await Mediator.Send(new UpdateSalesAggregates.Command
        {
            PeriodType = periodType,
            ProductIds = productIds
        }));
    }
}
