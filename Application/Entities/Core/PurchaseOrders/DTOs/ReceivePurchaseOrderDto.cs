namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class ReceivePurchaseOrderDto
{
    public Guid CompanyId { get; set; }
    public ICollection<ReceivePurchaseOrderItemDto> lineItems { get; set; } = new List<ReceivePurchaseOrderItemDto>();
}
