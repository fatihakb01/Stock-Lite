namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class ReceivePurchaseOrderDto
{
    public ICollection<ReceivePurchaseOrderItemDto> lineItems { get; set; } = new List<ReceivePurchaseOrderItemDto>();
}
