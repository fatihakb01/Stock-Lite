using Application.Entities.Core.TransactionLineItems.DTOs;

namespace Application.Entities.Core.Transactions.DTOs;

public class TransactionDto : BaseTransactionDto
{
    public Guid Id { get; set; }
    public ICollection<TransactionLineItemDto> LineItems { get; set; }
        = new List<TransactionLineItemDto>();
}
