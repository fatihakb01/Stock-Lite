namespace Application.Entities.Core.Products.DTOs;

public class ProductDetailDto : BaseProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }

    // extra product info
    public ProductStockDto? StockLevel { get; set; }
    public ProductInventoryPolicyDto? InventoryPolicy { get; set; }
}
