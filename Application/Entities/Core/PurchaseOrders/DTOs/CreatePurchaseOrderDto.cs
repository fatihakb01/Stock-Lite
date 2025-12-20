namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class CreatePurchaseOrderDto : BasePurchaseOrderDto
{
    public ICollection<CreatePurchaseOrderLineItemDto> LineItems { get; set; }
        = new List<CreatePurchaseOrderLineItemDto>();
}
