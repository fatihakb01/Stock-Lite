using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProducts
{
    public class Query : IRequest<Result<List<ProductListDto>>> {}

    public class Handler(
        IRepository<Product> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ProductListDto>>>
    {
        public async Task<Result<List<ProductListDto>>> Handle(Query request, CancellationToken ct)
        {
            var products = await repository.FindAsync(x => 
                x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (products == null || !products.Any())
                return Result<List<ProductListDto>>.Failure("No products found for this company", 404);

            var productDtos = mapper.Map<List<ProductListDto>>(products);

            return Result<List<ProductListDto>>.Success(productDtos);
        }
    }
}
