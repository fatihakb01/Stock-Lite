using Application.Entities.Core.TransactionLineItems.DTOs;
using Application.Entities.Core.TransactionLineItems.Features.Commands;
using Application.Entities.Core.TransactionLineItems.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TransactionLineItemController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<List<TransactionLineItemDto>>> GetTransactionLineItem(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetTransactionLineItem.Query{ Id = id }));
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<ActionResult<TransactionLineItemDto>> GetTransactionsByCustomerId(Guid transactionId)
    {
        return HandleResult(await Mediator.Send(
            new GetTransactionLineItemsByTransactionId.Query { TransactionId = transactionId }
        ));
    }        

    [HttpGet]
    public async Task<ActionResult<List<TransactionLineItemDto>>> GetTransactionLineItems()
    {
        return HandleResult(await Mediator.Send(new GetTransactionLineItems.Query()));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionLineItemDto>> CreateTransactionLineItem(
        [FromBody] CreateTransactionLineItemDto transactionLineItemDtoDto)
    {
        return HandleResult(
            await Mediator.Send(
                new CreateTransactionLineItem.Command
                {
                    TransactionLineItemDto = transactionLineItemDtoDto
                }
        ));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionLineItemDto>> EditTransactionLineItem(
        Guid id,
        [FromBody] EditTransactionLineItemDto transactionLineItemDto)
    {
        return HandleResult(await Mediator.Send(new EditTransactionLineItem.Command
        {
            Id = id,
            TransactionLineItemDto = transactionLineItemDto
        }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteTransaction(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteTransactionLineItem.Command { Id = id }));
    }
}
