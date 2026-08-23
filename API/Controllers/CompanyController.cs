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
    [HttpGet]
    public async Task<ActionResult<CompanyDto>> GetCompany()
    {
        return HandleResult(await Mediator.Send(new GetCompany.Query()));
    }

    //[Authorize(Policy = "SuperAdminOnly")]
    //[HttpPost]
    //public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyDto companyDto)
    //{
    //    return HandleResult(await Mediator.Send(new CreateCompany.Command { CompanyDto = companyDto }));
    //}

    //[Authorize(Policy = "SuperAdminOnly")]
    //[HttpPut]
    //public async Task<ActionResult<CompanyDto>> EditCompany([FromBody] EditCompanyDto companyDto)
    //{
    //    return HandleResult(await Mediator.Send(new EditCompany.Command { CompanyDto = companyDto }));
    //}
    
    // [Authorize(Policy = "SuperAdminOnly")]
    // [HttpDelete]
    // public async Task<ActionResult<Unit>> DeleteCompany()
    // {
    //     return HandleResult(await Mediator.Send(new DeleteCompany.Command()));
    // }
}
