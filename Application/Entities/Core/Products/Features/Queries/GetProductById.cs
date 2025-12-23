using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProductById
{
    public class Query : IRequest<Result<ProductDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(
        IRepository<Product> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(Query request, CancellationToken ct)
        {
            var products = await repository.FindAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                s => s.StockLevel!
            );
            var product = products.SingleOrDefault();

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            var productDto = mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Success(productDto);
        }
    }
}
