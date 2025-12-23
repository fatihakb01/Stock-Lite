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
            var products = await repository.FindAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct
            );
            var product = products.SingleOrDefault();

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            mapper.Map(request.ProductDto, product);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<ProductDto>.Failure("Failed to update product", 400);

            var updatedProductDto = mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Success(updatedProductDto);
        }
    }
}
