namespace Application.Entities.Core.Customers.DTOs;

public class BaseCustomerDto
{
    public string DisplayName { get; set; } = "";
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid CompanyId { get; set; }
}
