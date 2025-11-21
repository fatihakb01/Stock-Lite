using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProductsByCompanyId
{
    public class Query : IRequest<Result<List<ProductDto>>>
    {
        public Guid CompanyId { get; set; }
    }

    public class Handler(IRepository<Product> repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var products = await repository
                .FindAsync(x => x.CompanyId == request.CompanyId, cancellationToken);

            if (products == null || !products.Any())
                return Result<List<ProductDto>>.Failure("No products found for this company", 404);

            var productDtos = mapper.Map<List<ProductDto>>(products);

            return Result<List<ProductDto>>.Success(productDtos);
        }
    }
}
