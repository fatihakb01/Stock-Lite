using Application.Entities.Users.DTOs;
using Application.Entities.Users.Features.Commands;
using Application.Entities.Users.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto loginUserDto)
    {
        return HandleResult(await Mediator.Send(new LoginUser.Command { LoginUserDto = loginUserDto }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto registerUserDto)
    {
        return HandleResult(await Mediator.Send(new RegisterUser.Command { RegisterUserDto = registerUserDto }));
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        return HandleResult(await Mediator.Send(new GetCurrentUser.Query()));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetUserById.Query { UserId = id }));
    }
}
