namespace Application.Entities.Core.Products.DTOs;

public class ProductStockDto
{
    public int QuantityOnHand { get; set; }
    public int QuantityOnOrder { get; set; }
    public int ReorderPoint { get; set; }
}
