using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
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
        IBaseCacheService<Product> cacheService,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ProductListDto>>>
    {
        public async Task<Result<List<ProductListDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(currentUser.CompanyId, ProductKey.Products);
            cacheKey = request.Filter != null ? cacheKey + ":" + request.Filter : cacheKey;
            var cached = await cacheService.GetAsync<List<ProductListDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<ProductListDto>>.Success(cached);

            var products = await repository.GetFilteredProducts(request.Filter, currentUser.CompanyId, ct);

            if (products.Count == 0)
                return Result<List<ProductListDto>>.Failure("No products found for this company", 404);

            var productDtos = mapper.Map<List<ProductListDto>>(products);

            await cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(5), ct);

            return Result<List<ProductListDto>>.Success(productDtos);
        }
    }
}
