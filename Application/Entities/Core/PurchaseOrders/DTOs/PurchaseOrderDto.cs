using Domain.Enums;

namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class PurchaseOrderDto : BasePurchaseOrderDto
{
    public Guid Id { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public ICollection<PurchaseOrderLineItemDto> LineItems { get; set; }
        = new List<PurchaseOrderLineItemDto>();
}
