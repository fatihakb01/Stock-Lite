using Application.Entities.Core.Transactions.DTOs;
using Application.Entities.Core.Transactions.Features.Commands;
using Application.Entities.Core.Transactions.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class TransactionController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("by-product/{productId}")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsByProductId(Guid productId)
    {
        return HandleResult(await Mediator.Send(new GetTransactionsByProductId.Query { ProductId = productId }));
    }  

    //[HttpGet]
    //public async Task<ActionResult<List<TransactionDto>>> GetTransactions()
    //{
    //    return HandleResult(await Mediator.Send(new GetTransactions.Query()));
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPost]
    //public async Task<ActionResult<TransactionDto>> CreateTransaction(
    //    [FromBody] CreateTransactionDto transactionDto)
    //{
    //    return HandleResult(await Mediator.Send(new CreateTransaction.Command { TransactionDto = transactionDto }));
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpPut("{id}")]
    //public async Task<ActionResult<TransactionDto>> EditTransaction(Guid id,
    //    [FromBody] EditTransactionDto transactionDto)
    //{
    //    return HandleResult(await Mediator.Send(new EditTransaction.Command
    //    {
    //        Id = id,
    //        TransactionDto = transactionDto
    //    }));
    //}

    //[Authorize(Policy = "AdminOrSuperAdmin")]
    //[HttpDelete("{id}")]
    //public async Task<ActionResult<Unit>> DeleteTransaction(Guid id)
    //{
    //    return HandleResult(await Mediator.Send(new DeleteTransaction.Command { Id = id }));
    //}
}
