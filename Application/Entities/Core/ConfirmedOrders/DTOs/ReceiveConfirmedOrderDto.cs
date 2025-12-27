namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class ReceiveConfirmedOrderDto
{
    public ICollection<ReceiveConfirmedOrderItemDto> LineItems { get; set; } = new List<ReceiveConfirmedOrderItemDto>();
}
