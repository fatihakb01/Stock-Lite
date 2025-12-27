namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class BaseConfirmedOrderLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int SoldQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
