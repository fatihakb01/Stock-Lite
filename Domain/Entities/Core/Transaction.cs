namespace Domain.Entities.Core;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string TransactionId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; } 
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    // Foreign keys
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public ICollection<TransactionLineItem> LineItems { get; set; } = new List<TransactionLineItem>();
}
