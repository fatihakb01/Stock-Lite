using Application.Entities.SalesAggregates.Features.Commands;
using Application.Entities.SalesAggregates.Features.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

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

    [HttpPost("{companyId}/{periodType?}")]
    public async Task<ActionResult<SalesAggregate>> UpdateSalesAggregates
        (Guid companyId, string? periodType, [FromBody] IEnumerable<Guid>? productIds)
    {
        return HandleResult(await Mediator.Send(new UpdateSalesAggregates.Command
        {
            CompanyId = companyId,
            PeriodType = periodType,
            ProductIds = productIds
        }));
    }
}
