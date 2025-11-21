namespace Application.Entities.Users.DTOs;

public class ChangePasswordDto
{
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}
