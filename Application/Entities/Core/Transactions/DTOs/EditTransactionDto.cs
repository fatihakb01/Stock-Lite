using Application.Entities.Core.TransactionLineItems.DTOs;

namespace Application.Entities.Core.Transactions.DTOs;

public class EditTransactionDto : BaseTransactionDto
{
    public ICollection<EditTransactionLineItemDto> LineItems { get; set; }
        = new List<EditTransactionLineItemDto>();
}
