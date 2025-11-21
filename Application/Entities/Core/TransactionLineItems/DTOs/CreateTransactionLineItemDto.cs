namespace Application.Entities.Core.TransactionLineItems.DTOs;

public class CreateTransactionLineItemDto : BaseTransactionLineItemDto
{
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
}
