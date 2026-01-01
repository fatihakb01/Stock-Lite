using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProductById
{
    public class Query : IRequest<Result<ProductDetailDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(
        IRepository<Product> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ProductDetailDto>>
    {
        public async Task<Result<ProductDetailDto>> Handle(Query request, CancellationToken ct)
        {
            var products = await repository.FindAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                s => s.StockLevel!,
                s => s.InventoryPolicy!
            );

            var product = products.SingleOrDefault();

            if (product == null)
                return Result<ProductDetailDto>.Failure("Product not found", 404);

            var productDto = mapper.Map<ProductDetailDto>(product);

            return Result<ProductDetailDto>.Success(productDto);
        }
    }
}
