namespace Infrastructure.Services.Scheduler.Imports.Records;

public sealed class TransactionRecord
{
    public required string TransactionId { get; set; }
    public required DateTime TransactionDate { get; set; }
    public required string SKU { get; set; }
    public required int Quantity { get; set; }

    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}
