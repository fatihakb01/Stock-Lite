using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Core;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign Key
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
}
