using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProducts
{
    public class Query : IRequest<Result<List<ProductListDto>>>
    {
        public ProductFilter? Filter { get; set; }
    }

    public class Handler(
        IProductRepository repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ProductListDto>>>
    {
        public async Task<Result<List<ProductListDto>>> Handle(Query request, CancellationToken ct)
        {
            var products = await repository.GetFilteredProducts(request.Filter, currentUser.CompanyId, ct);

            if (products.Count == 0)
                return Result<List<ProductListDto>>.Failure("No products found for this company", 404);

            var productDtos = mapper.Map<List<ProductListDto>>(products);

            return Result<List<ProductListDto>>.Success(productDtos);
        }
    }
}
