namespace Application.Entities.Core.Users.DTOs;

public class RegisterUserDto
{
    public required string UserName { get; set; } = "";
    public required string Email { get; set; } = "";
    public required string Password { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public Guid CompanyId { get; set; }
}
