namespace Domain.Entities.Core;

public class StockLevel
{
    public Guid ProductId { get; set; }

    public int QuantityOnHand { get; set; }
    public int QuantityOnOrder { get; set; }
    public int ReorderPoint { get; set; } 

    // Navigation property
    public required Product Product { get; set; }
}
