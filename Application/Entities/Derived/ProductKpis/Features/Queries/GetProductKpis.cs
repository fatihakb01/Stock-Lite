using Application.Core;
using Application.Entities.Derived.ProductKpis.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ProductKpis.Features.Queries;

public class GetProductKpis
{
    public class Query : IRequest<Result<ProductKpiDto>> { }

    public class Handler(
        IProductKpiRepository repository, 
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<ProductKpiDto>>
    {
        public async Task<Result<ProductKpiDto>> Handle(Query request, CancellationToken ct)
        {
            var productKpi = await repository.GetProductKpis(currentUser.CompanyId, ct);

            if (productKpi == null)
                return Result<ProductKpiDto>.Failure("ProductKpi not found", 404);

            var productKpiDto = mapper.Map<ProductKpiDto>(productKpi);

            return Result<ProductKpiDto>.Success(productKpiDto);
        }
    }
}
