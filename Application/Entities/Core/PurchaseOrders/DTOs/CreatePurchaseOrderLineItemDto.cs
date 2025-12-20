namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class CreatePurchaseOrderLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
