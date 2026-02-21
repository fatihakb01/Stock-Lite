using Application.Entities.Core.UserDefinedFields.DTOs;
using Application.Entities.Core.UserDefinedFields.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UserDefinedFieldController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{productId}")]
    public async Task<ActionResult<UserDefinedFieldDto>> GetUserDefinedFieldsByProductId(Guid productId)
    {
        return HandleResult( await Mediator.Send(
                new GetUserDefinedFieldsByProductId.Query
                {
                    ProductId = productId
                }
            )
        );
    }
}
