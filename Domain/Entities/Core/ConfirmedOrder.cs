using Domain.Enums;

namespace Domain.Entities.Core;

public class ConfirmedOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OrderNumber { get; set; }
    public ConfirmedOrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    // Foreign keys
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public ICollection<ConfirmedOrderLineItem> LineItems { get; set; } = new List<ConfirmedOrderLineItem>();
}
