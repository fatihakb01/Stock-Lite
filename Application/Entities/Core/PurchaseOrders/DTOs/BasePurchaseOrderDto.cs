namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class BasePurchaseOrderDto
{
    public Guid CompanyId { get; set; }
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}
