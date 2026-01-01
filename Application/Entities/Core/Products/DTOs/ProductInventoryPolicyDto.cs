namespace Application.Entities.Core.Products.DTOs;

public class ProductInventoryPolicyDto
{
    public int LeadTimeDays { get; set; }

    public int MinimumOrderQuantity { get; set; }
    public int OrderIncrementQuantity { get; set; }

    public int SafetyStock { get; set; }
    public int StockAdjustment { get; set; }
}
