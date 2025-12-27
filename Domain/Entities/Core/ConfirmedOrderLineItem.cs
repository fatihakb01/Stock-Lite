namespace Domain.Entities.Core;

public class ConfirmedOrderLineItem
{
    public Guid ConfirmedOrderId { get; set; }
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    public int SoldQuantity { get; set; }
    public decimal UnitCost { get; set; }

    // Navigation
    public required ConfirmedOrder ConfirmedOrder { get; set; }
    public required Product Product { get; set; }
}
