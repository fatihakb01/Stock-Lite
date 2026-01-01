using Domain.Entities.Core;

namespace Domain.Entities.Derived;

public class StockLevel
{
    public Guid ProductId { get; set; }

    public int QuantityOnHand { get; set; }
    public int QuantityOnOrder { get; set; }
    public int QuantityCommitted { get; set; }

    public int NetStockPosition { get; set; }
    public int ProjectedStockAtLeadTime { get; set; }

    public int ReorderPoint { get; set; }

    public bool NeedsReorder { get; set; }
    public int RecommendedOrderQuantity { get; set; }

    public DateTime LastCalculatedAt { get; set; }

    // Navigation
    public required Product Product { get; set; }
}
