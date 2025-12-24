using Application.Entities.Core.TransactionLineItems.DTOs;

namespace Application.Entities.Core.Transactions.DTOs;

public class CreateTransactionDto : BaseTransactionDto
{
    public ICollection<CreateTransactionLineItemDto> LineItems { get; set; }
        = new List<CreateTransactionLineItemDto>();
}
