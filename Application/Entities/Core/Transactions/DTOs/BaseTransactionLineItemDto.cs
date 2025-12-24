namespace Application.Entities.Core.TransactionLineItems.DTOs;

public class BaseTransactionLineItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
