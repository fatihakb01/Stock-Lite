namespace Application.Entities.Core.Products.DTOs;

public class ProductStockDto
{
    public int QuantityOnHand { get; set; }
    public int QuantityOnOrder { get; set; }
    public int QuantityCommitted { get; set; }

    public decimal NetStockPosition { get; set; }
    public decimal ForecastQuantity { get; set; }

    public int ReorderPoint { get; set; }

    public bool NeedsReorder { get; set; }
    public int RecommendedOrderQuantity { get; set; }

    public DateTime LastCalculatedAt { get; set; }
}
