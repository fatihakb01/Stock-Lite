namespace Infrastructure.Services.Imports.Records;

public sealed class ProductRecord
{
    public required string Name { get; set; }
    public required string SKU { get; set; }
    public required decimal Price { get; set; }
    public required int LeadTimeDays { get; set; }
    
    public int MinimumOrderQuantity { get; set; }
    public int OrderIncrementQuantity { get; set; }
    public int SafetyStock { get; set; }
    public int StockAdjustment { get; set; }

    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
