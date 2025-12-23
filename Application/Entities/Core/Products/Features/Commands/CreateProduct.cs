using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Commands;

public class CreateProduct
{
    public class Command : IRequest<Result<ProductDto>>
    {
        public required CreateProductDto ProductDto { get; set; }
    }

    public class Handler(
        IRepository<Product> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = mapper.Map<Product>(request.ProductDto);
            product.CompanyId = currentUser.CompanyId;

            await repository.AddAsync(product, cancellationToken);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<ProductDto>.Failure("Failed to create product", 400);

            var createdProductDto = mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Success(createdProductDto);
        }
    }
}
