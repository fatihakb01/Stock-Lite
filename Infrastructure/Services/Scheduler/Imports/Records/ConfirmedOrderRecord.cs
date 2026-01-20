namespace Infrastructure.Services.Scheduler.Imports.Records;

public sealed class ConfirmedOrderRecord
{
    public required string OrderNumber { get; set; }
    public required DateTime OrderDate { get; set; }
    public required DateTime ExpectedDeliveryDate { get; set; }

    public required string SKU { get; set; }
    public required int Quantity { get; set; }
    public required int SoldQuantity { get; set; }

    public DateTime? ReceivedDate { get; set; }
}
