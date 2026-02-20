using Application.Core;
using Application.Entities.Core.Products.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Products.Features.Queries;

public class GetProductById
{
    public class Query : IRequest<Result<ProductDetailDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(
        IProductRepository repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<ProductDetailDto>>
    {
        public async Task<Result<ProductDetailDto>> Handle(Query request, CancellationToken ct)
        {
            var product = await repository.SingleOrDefaultAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                ct,
                s => s.StockLevel!,
                s => s.InventoryPolicy!
            );

            if (product == null)
                return Result<ProductDetailDto>.Failure("Product not found", 404);

            var productDto = mapper.Map<ProductDetailDto>(product);

            return Result<ProductDetailDto>.Success(productDto);
        }
    }
}
