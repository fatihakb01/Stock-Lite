using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProduct
{
    public class Query : IRequest<Result<ProductDto>>
    {
        public required Guid Id { get; set; }
        public required Guid CompanyId { get; set; }
    }

    public class Handler(IProductRepository repository, IMapper mapper)
        : IRequestHandler<Query, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await repository.GetWithStockAsync(request.Id, request.CompanyId);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            var productDto = mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Success(productDto);
        }
    }
}
