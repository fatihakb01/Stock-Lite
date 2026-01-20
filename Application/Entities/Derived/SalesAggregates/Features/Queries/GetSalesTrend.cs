using Application.Core;
using Application.Entities.Derived.SalesAggregates.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.SalesAggregates.Features.Queries;

public class GetSalesTrend
{
    public class Query : IRequest<Result<List<SalesAggregateDto>>>
    {
        public Guid ProductId { get; set; }
        public string PeriodType { get; set; } = "Daily";
    }

    public class Handler(
        ISalesAggregateRepository salesaggregateRepo, 
        IRepository<Product> productRepo,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<SalesAggregateDto>>>
    {
        public async Task<Result<List<SalesAggregateDto>>> Handle(Query request, CancellationToken ct)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<List<SalesAggregateDto>>.Failure($"Invalid period type: {request.PeriodType}", 400);

            var products = await productRepo.FindAsync(x =>
                x.Id == request.ProductId && x.CompanyId == currentUser.CompanyId,
                ct
            );
            
            if (products == null || !products.Any())
                return Result<List<SalesAggregateDto>>.Failure($"Product not found for this company", 404);

            var salesAggregate = await salesaggregateRepo.GetByProductIdAsync(request.ProductId, parsedPeriod);

            if (salesAggregate == null || !salesAggregate.Any())
                return Result<List<SalesAggregateDto>>.Failure("No history available", 404);

            var salesAggregateDto = mapper.Map<List<SalesAggregateDto>>(salesAggregate);

            return Result<List<SalesAggregateDto>>.Success(salesAggregateDto);
        }
    }
}
