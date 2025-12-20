namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class BasePurchaseOrderLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
