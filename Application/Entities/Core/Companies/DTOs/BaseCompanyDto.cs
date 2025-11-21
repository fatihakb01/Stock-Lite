namespace Application.Entities.Core.Companies.DTOs;

public class BaseCompanyDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
