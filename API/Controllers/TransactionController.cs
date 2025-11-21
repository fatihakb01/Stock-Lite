using Application.Entities.Core.Transactions.DTOs;
using Application.Entities.Core.Transactions.Features.Commands;
using Application.Entities.Core.Transactions.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TransactionController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetTransaction.Query { Id = id }));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionsByCustomerId(Guid customerId)
    {
        return HandleResult(await Mediator.Send(new GetTransactionsByCustomerId.Query { CustomerId = customerId }));
    }    

    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactions()
    {
        return HandleResult(await Mediator.Send(new GetTransactions.Query()));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        [FromBody] CreateTransactionDto transactionDto)
    {
        return HandleResult(await Mediator.Send(new CreateTransaction.Command { TransactionDto = transactionDto }));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> EditTransaction(Guid id,
        [FromBody] EditTransactionDto transactionDto)
    {
        return HandleResult(await Mediator.Send(new EditTransaction.Command
        {
            Id = id,
            TransactionDto = transactionDto
        }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteTransaction(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteTransaction.Command { Id = id }));
    }
}
