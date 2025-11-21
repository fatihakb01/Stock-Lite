using Application.Entities.Core.Customers.DTOs;
using Application.Entities.Core.Customers.Features.Commands;
using Application.Entities.Core.Customers.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CustomerController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetCustomer.Query { Id = id }));
    }

    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerByCompanyId(Guid companyId)
    {
        return HandleResult(await Mediator.Send(new GetCustomerByCompanyId.Query { CompanyId = companyId }));
    }    

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        return HandleResult(await Mediator.Send(new GetCustomers.Query()));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto customerDto)
    {
        return HandleResult(await Mediator.Send(new CreateCustomer.Command { CustomerDto = customerDto }));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> EditCustomer(Guid id, [FromBody] EditCustomerDto customerDto)
    {
        return HandleResult(await Mediator.Send(new EditCustomer.Command { Id = id, CustomerDto = customerDto }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteCustomer(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteCustomer.Command { Id = id }));
    }
}
