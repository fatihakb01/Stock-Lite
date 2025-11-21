namespace Application.Entities.Core.TransactionLineItems.DTOs;

public class TransactionLineItemDto : BaseTransactionLineItemDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
}
