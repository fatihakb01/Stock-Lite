namespace Domain.Entities.Core;

public class StockLevel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
    public int ReorderPoint { get; set; } 

    // Navigation property
    public required Product Product { get; set; }
}
