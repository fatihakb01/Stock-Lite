using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Commands;

public class EditProduct
{
    public class Command : IRequest<Result<ProductDto>>
    {
        public Guid Id { get; set; }
        public required EditProductDto ProductDto { get; set; }
    }

    public class Handler(
        IRepository<Product> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ProductDto;

            var products = await repository.FindAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct
            );
            var product = products.SingleOrDefault();

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            var skuExist = await repository.ExistsAsync(x => 
                x.Id != request.Id && 
                x.SKU == dto.SKU && 
                x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (skuExist)
                return Result<ProductDto>.Failure("SKU must be unique", 400);

            var nameExist = await repository.ExistsAsync(x => 
                x.Id != request.Id && 
                x.Name == dto.Name && 
                x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (nameExist)
                return Result<ProductDto>.Failure("Name must be unique", 400);

            mapper.Map(dto, product);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<ProductDto>.Failure("Failed to update product", 400);

            var updatedProductDto = mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Success(updatedProductDto);
        }
    }
}
