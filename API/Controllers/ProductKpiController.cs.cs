using Application.Entities.Derived.ProductKpis.DTOs;
using Application.Entities.Derived.ProductKpis.Features.Commands;
using Application.Entities.Derived.ProductKpis.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ProductKpiController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<ActionResult<ProductKpiDto>> GetProductKpis()
    {
        return HandleResult(await Mediator.Send(new GetProductKpis.Query()));
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> UpdateProductKpis()
    {
        return HandleResult(await Mediator.Send(new UpdateProductKpis.Command()));
    }
}
