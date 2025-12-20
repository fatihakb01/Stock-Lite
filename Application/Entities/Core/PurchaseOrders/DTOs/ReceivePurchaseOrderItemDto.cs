namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class ReceivePurchaseOrderItemDto
{
    public Guid ProductId { get; set; }
    public int QuantityReceived { get; set; }
}
