using Application.Entities.Derived.Seasonality.DTOs;
using Application.Entities.Derived.Seasonality.Features.Commands;
using Application.Entities.Derived.Seasonality.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class SeasonalityController(IMediator mediator) : BaseApiController(mediator)
{
    //[HttpGet("{productId}")]
    //public async Task<ActionResult<List<SeasonalityDto>>> GetSeasonality(Guid productId)
    //{
    //    return HandleResult(await Mediator.Send(new GetSeasonality.Query { ProductId = productId }));
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost]
    //public async Task<ActionResult<Unit>> UpdateSeasonality([FromBody] IEnumerable<Guid>? productIds)
    //{
    //    return HandleResult(await Mediator.Send(new UpdateSeasonality.Command { ProductIds = productIds }));
    //}
}
