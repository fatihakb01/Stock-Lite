namespace Application.Entities.Core.Products.DTOs;

public class ProductListDto : BaseProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
}
