namespace Application.Entities.Core.Products.DTOs;

public class ProductDto : BaseProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }

    // Stock info
    public ProductStockDto? Stock { get; set; }
}
