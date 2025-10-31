using System;

namespace Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; } 
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }

    // Foreign Keys
    public Guid CustomerId { get; set; }

    // Navigation
    public required Customer Customer { get; set; }
    public ICollection<TransactionLineItem> LineItems { get; set; } = new List<TransactionLineItem>();
}
