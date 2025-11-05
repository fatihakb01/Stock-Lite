using Application.Entities.Products.DTOs;
using Application.Entities.Products.Features.Commands;
using Application.Entities.Products.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, [FromQuery] Guid companyId)
    {
        return HandleResult(await Mediator.Send(new GetProduct.Query { Id = id, CompanyId = companyId }));
    }

    [HttpGet]
    public async Task<ActionResult<ProductDto>> GetProducts([FromQuery] Guid companyId)
    {
        return HandleResult(await Mediator.Send(new GetProductsByCompanyId.Query { CompanyId = companyId }));
    }    
    
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductDto productDto)
    {
        return HandleResult(await Mediator.Send(new CreateProduct.Command { ProductDto = productDto }));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> EditProduct(Guid id,
        [FromBody] EditProductDto productDto)
    {
        return HandleResult(await Mediator.Send(new EditProduct.Command
        {
            Id = id,
            ProductDto = productDto
        }));
    }

    // [HttpDelete("{id}")]
    // public async Task<ActionResult<Unit>> DeleteProduct(Guid id,[FromQuery] Guid companyId)
    // {
    //     return HandleResult(await Mediator.Send(new DeleteProduct.Command { Id = id, CompanyId = companyId }));
    // }
}
