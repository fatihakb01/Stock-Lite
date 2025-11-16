using Domain.Enums;

namespace Application.Entities.Users.DTOs;

public class UserDto
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; }
    public Guid CompanyId { get; set; }
}
