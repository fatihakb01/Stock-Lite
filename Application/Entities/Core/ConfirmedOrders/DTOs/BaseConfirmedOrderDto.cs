namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class BaseConfirmedOrderDto
{
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}
