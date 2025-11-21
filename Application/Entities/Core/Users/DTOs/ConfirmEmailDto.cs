namespace Application.Entities.Core.Users.DTOs;

public class ConfirmEmailDto
{
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
}
