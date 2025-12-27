
namespace Application.Entities.Core.ConfirmedOrders.DTOs;

public class CreateConfirmedOrderDto : BaseConfirmedOrderDto
{
    public ICollection<CreateConfirmedOrderLineItemDto> LineItems { get; set; }
        = new List<CreateConfirmedOrderLineItemDto>();
}
