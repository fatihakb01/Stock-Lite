namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class UpdatePurchaseOrderDto : BasePurchaseOrderDto
{
    public ICollection<UpdatePurchaseOrderLineItemDto> LineItems { get; set; }
        = new List<UpdatePurchaseOrderLineItemDto>();
}
