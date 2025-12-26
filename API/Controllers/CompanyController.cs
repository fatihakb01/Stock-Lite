using Application.Entities.Core.Companies.DTOs;
using Application.Entities.Core.Companies.Features.Commands;
using Application.Entities.Core.Companies.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class CompanyController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetCompany.Query { Id = id }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyDto companyDto)
    {
        return HandleResult(await Mediator.Send(new CreateCompany.Command { CompanyDto = companyDto }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> EditCompany(Guid id, [FromBody] EditCompanyDto companyDto)
    {
        return HandleResult(await Mediator.Send(new EditCompany.Command { Id = id, CompanyDto = companyDto }));
    }
    
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteCompany.Command { Id = id }));
    }
}
