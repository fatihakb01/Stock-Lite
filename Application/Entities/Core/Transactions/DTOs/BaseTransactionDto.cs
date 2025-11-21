namespace Application.Entities.Core.Transactions.DTOs;

public class BaseTransactionDto
{
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public Guid CustomerId { get; set; }
}
