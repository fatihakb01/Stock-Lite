using Application.Core;
using Application.Entities.Derived.ProductKpis.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ProductKpis.Features.Queries;

public class GetProductKpis
{
    public class Query : IRequest<Result<ProductKpiDto>> { }

    public class Handler(
        IProductKpiRepository repository,
        ICacheService cacheService,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<ProductKpiDto>>
    {
        public async Task<Result<ProductKpiDto>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = $"productkpis:{currentUser.CompanyId}";

            var cached = await cacheService.GetAsync<ProductKpiDto>(cacheKey, ct);
            if (cached != null)
                return Result<ProductKpiDto>.Success(cached);

            var productKpi = await repository.GetProductKpis(currentUser.CompanyId, ct);
            if (productKpi == null)
                return Result<ProductKpiDto>.Failure("ProductKpi not found", 404);

            var productKpiDto = mapper.Map<ProductKpiDto>(productKpi);

            await cacheService.SetAsync(cacheKey, productKpiDto, TimeSpan.FromMinutes(5), ct);

            return Result<ProductKpiDto>.Success(productKpiDto);
        }
    }
}
