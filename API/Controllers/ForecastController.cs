using Application.Entities.Derived.Forecasts.DTOs;
using Application.Entities.Derived.Forecasts.Features.Commands;
using Application.Entities.Derived.Forecasts.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ForecastController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{periodType}/{productId}")]
    public async Task<ActionResult<List<ForecastDto>>> GetForecasts(string periodType, Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetForecasts.Query
        {
            ProductId = productId,
            PeriodType = periodType
        }));
    }
    
    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost("{periodType?}")]
    public async Task<ActionResult<Unit>> GenerateForecasts
        (string? periodType, [FromBody] IEnumerable<Guid>? productIds)
    {
        return HandleResult(await Mediator.Send(new GenerateForecasts.Command
        {
            PeriodType = periodType,
            ProductIds = productIds
        }));
    }
}
