using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProducts
{
    public class Query : IRequest<Result<List<ProductDto>>> { }

    public class Handler(IRepository<Product> repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var products = await repository.GetAllAsync(cancellationToken);

            if (products == null)
                return Result<List<ProductDto>>.Failure("Products not found", 404);

            var productsDto = mapper.Map<List<ProductDto>>(products);

            return Result<List<ProductDto>>.Success(productsDto);
        }
    }
}
