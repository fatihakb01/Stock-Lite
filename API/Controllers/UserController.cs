using Application.Entities.Core.Users.DTOs;
using Application.Entities.Core.Users.Features.Commands;
using Application.Entities.Core.Users.Features.Queries;
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

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("edit/{id}")]
    public async Task<ActionResult<UserDto>> EditUser(Guid id, [FromBody] EditUserDto editUserDto)
    {
        return HandleResult(await Mediator.Send(new EditUser.Command { Id = id, EditUserDto = editUserDto }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<Unit>> DeleteUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteUser.Command { Id = id }));
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<UserDto>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        return HandleResult(await Mediator.Send(new ConfirmEmail.Command { Dto = dto }));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<Unit>> ResetPassword([FromBody] ForgotPasswordDto dto)
    {
        return HandleResult(await Mediator.Send(new ResetPassword.Command { Dto = dto }));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<Unit>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        return HandleResult(await Mediator.Send(new ChangePassword.Command { Dto = dto }));
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        return HandleResult(await Mediator.Send(new GetAllUsers.Query { }));
    }

    [Authorize]
    [HttpGet("current")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        return HandleResult(await Mediator.Send(new GetCurrentUser.Query()));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetUserById.Query { UserId = id }));
    }
}
