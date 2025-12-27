namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class ReceiveConfirmedOrderItemDto
{
    public Guid ProductId { get; set; }
    public int QuantitySold { get; set; }
}
