using Domain.Enums;

namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class ConfirmedOrderDto : BaseConfirmedOrderDto
{
    public Guid Id { get; set; }
    public ConfirmedOrderStatus Status { get; set; }
    public ICollection<ConfirmedOrderLineItemDto> LineItems { get; set; }
        = new List<ConfirmedOrderLineItemDto>();
}
