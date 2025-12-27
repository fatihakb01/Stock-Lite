namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class CreateConfirmedOrderLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
