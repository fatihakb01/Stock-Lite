using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Products.Features.Commands;

public class CreateProduct
{
    public class Command : IRequest<Result<ProductListDto>>
    {
        public required CreateProductDto ProductDto { get; set; }
    }

    public class Handler(
        IProductRepository repository,
        IProductCacheService cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<ProductListDto>>
    {
        public async Task<Result<ProductListDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.ProductDto;

            var skuExist = await repository.ExistsAsync(x => 
                x.SKU == dto.SKU && x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (skuExist)
                return Result<ProductListDto>.Failure("SKU must be unique", 400);

            var nameExist = await repository.ExistsAsync(x => 
                x.Name == dto.Name && x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (nameExist)
                return Result<ProductListDto>.Failure("Name must be unique", 400);

            var product = mapper.Map<Product>(dto);
            product.CompanyId = currentUser.CompanyId;

            await repository.AddAsync(product, ct);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<ProductListDto>.Failure("Failed to create product", 400);

            var createdProductDto = mapper.Map<ProductListDto>(product);

            var cacheKey = cacheService.GetKey(currentUser.CompanyId, ProductKey.Products);
            await cacheService.RemoveAsync(cacheKey, ct);

            return Result<ProductListDto>.Success(createdProductDto);
        }
    }
}
