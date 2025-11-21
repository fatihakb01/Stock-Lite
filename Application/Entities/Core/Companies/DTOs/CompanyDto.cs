namespace Application.Entities.Core.Companies.DTOs;

public class CompanyDto : BaseCompanyDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
