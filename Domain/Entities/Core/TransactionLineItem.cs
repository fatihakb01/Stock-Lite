namespace Domain.Entities;

public class TransactionLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation
    public required Transaction Transaction { get; set; } 
    public required Product Product { get; set; }
}

