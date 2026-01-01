using Application.Core;
using Application.Entities.Derived.StockLevels.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Derived.StockLevels.Features.Queries;

public class GetStockLevel
{
    public class Query : IRequest<Result<StockLevelDto>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IStockLevelRepository stockLevelRepo,
        IRepository<Product> productRepo,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<StockLevelDto>>
    {
        public async Task<Result<StockLevelDto>> Handle(Query request, CancellationToken ct)
        {
            var products = await productRepo.FindAsync(x =>
                x.Id == request.ProductId && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!products.Any())
                return Result<StockLevelDto>.Failure($"Product not found for this company", 404);

            var stockLevel = await stockLevelRepo.GetByProductAsync(request.ProductId);

            if (stockLevel == null)
                return Result<StockLevelDto>.Failure($"No stock level available", 404);

            var stockLevelDto = mapper.Map<StockLevelDto>(stockLevel);

            return Result<StockLevelDto>.Success(stockLevelDto);
        }
    }
}
