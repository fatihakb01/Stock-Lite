namespace Domain.Entities.Core;

public class PurchaseOrderLineItem
{
    // Foreign keys
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }

    // Navigation
    public required PurchaseOrder PurchaseOrder { get; set; }
    public required Product Product { get; set; }
}

