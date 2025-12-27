namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class UpdateConfirmedOrderDto : BaseConfirmedOrderDto
{
    public ICollection<UpdateConfirmedOrderLineItemDto> LineItems { get; set; }
        = new List<UpdateConfirmedOrderLineItemDto>();
}
