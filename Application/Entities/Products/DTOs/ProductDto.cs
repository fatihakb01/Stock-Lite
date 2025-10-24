namespace Application.Entities.Products.DTOs;

public class ProductDto : BaseProductDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }

    // Stock info
    public ProductStockDto? Stock { get; set; }
}
