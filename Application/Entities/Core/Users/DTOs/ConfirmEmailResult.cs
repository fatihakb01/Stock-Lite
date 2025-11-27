namespace Application.Entities.Core.Users.DTOs;

public class ConfirmEmailResult
{
    public string Email { get; set; } = default!;
    public bool AlreadyConfirmed { get; set; }
}
