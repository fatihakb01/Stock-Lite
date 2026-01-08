namespace Application.Entities.Core.PurchaseOrders.DTOs;

public class BasePurchaseOrderDto
{
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
}
