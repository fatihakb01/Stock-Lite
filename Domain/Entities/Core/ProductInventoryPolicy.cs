namespace Domain.Entities.Core;

public class ProductInventoryPolicy
{
    public Guid ProductId { get; set; }

    public int LeadTimeDays { get; set; }

    public int MinimumOrderQuantity { get; set; }
    public int OrderIncrementQuantity { get; set; }

    public int SafetyStock { get; set; }
    public int StockAdjustment { get; set; }

    // Navigation
    public required Product Product { get; set; }
}
