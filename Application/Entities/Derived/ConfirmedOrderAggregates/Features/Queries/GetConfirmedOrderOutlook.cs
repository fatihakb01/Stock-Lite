using Application.Core;
using Application.Entities.Derived.ConfirmedOrderAggregates.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ConfirmedOrderAggregates.Features.Queries;

public class GetConfirmedOrderOutlook
{
    public class Query : IRequest<Result<List<ConfirmedOrderAggregateDto>>>
    {
        public Guid ProductId { get; set; }
        public string PeriodType { get; set; } = "Daily";
    }

    public class Handler(
        IConfirmedOrderAggregateRepository aggregateRepo,
        IRepository<Product> productRepo,
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Query, Result<List<ConfirmedOrderAggregateDto>>>
    {
        public async Task<Result<List<ConfirmedOrderAggregateDto>>> Handle(Query request, CancellationToken ct)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<List<ConfirmedOrderAggregateDto>>.Failure(
                    $"Invalid period type: {request.PeriodType}",
                    400
                );

            var products = await productRepo.FindAsync(
                x => x.Id == request.ProductId && x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (products == null || !products.Any())
                return Result<List<ConfirmedOrderAggregateDto>>.Failure(
                    "Product not found for this company",
                    404
                );

            var aggregates = await aggregateRepo.GetByProductIdAsync(
                request.ProductId,
                parsedPeriod
            );

            if (aggregates == null || !aggregates.Any())
                return Result<List<ConfirmedOrderAggregateDto>>.Failure(
                    "No confirmed orders available",
                    404
                );

            var dto = mapper.Map<List<ConfirmedOrderAggregateDto>>(aggregates);

            return Result<List<ConfirmedOrderAggregateDto>>.Success(dto);
        }
    }
}
