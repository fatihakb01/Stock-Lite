using Domain.Enums;

namespace Domain.Entities.Core;

public class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OrderNumber { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    
    // Foreign keys
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public ICollection<PurchaseOrderLineItem> LineItems { get; set; } = new List<PurchaseOrderLineItem>();
}
