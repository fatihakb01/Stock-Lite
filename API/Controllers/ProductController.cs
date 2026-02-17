using Application.Entities.Core.Products.DTOs;
using Application.Entities.Core.Products.Features.Commands;
using Application.Entities.Core.Products.Features.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class ProductController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetProductById.Query { Id = id }));
    }

    [HttpGet]
    public async Task<ActionResult<ProductListDto>> GetProducts([FromQuery] ProductFilter? filter)
    {
        return HandleResult(await Mediator.Send(new GetProducts.Query { Filter = filter }));
    }    
    
    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ProductListDto>> CreateProduct(
        [FromBody] CreateProductDto productDto)
    {
        return HandleResult(await Mediator.Send(new CreateProduct.Command { ProductDto = productDto }));
    }

    [Authorize(Policy = "AdminOrSuperAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductListDto>> EditProduct(Guid id,
        [FromBody] EditProductDto productDto)
    {
        return HandleResult(await Mediator.Send(new EditProduct.Command
        {
            Id = id,
            ProductDto = productDto
        }));
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteProduct(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteProduct.Command { Id = id }));
    }
}
