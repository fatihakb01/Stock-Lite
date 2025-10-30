using Application.Core;
using Application.Entities.SalesAggregates.DTOs;
using AutoMapper;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.SalesAggregates.Features.Queries;

public class GetSalesTrend
{
    public class Query : IRequest<Result<List<SalesAggregateDto>>>
    {
        public Guid ProductId { get; set; }
        public string PeriodType { get; set; } = "Daily";
    }

    public class Handler(ISalesAggregateRepository repository, IMapper mapper)
        : IRequestHandler<Query, Result<List<SalesAggregateDto>>>
    {
        public async Task<Result<List<SalesAggregateDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<Period>(request.PeriodType, true, out var parsedPeriod))
                return Result<List<SalesAggregateDto>>.Failure($"Invalid period type: {request.PeriodType}", 400);

            var salesAggregate = await repository.GetByProductIdAsync(request.ProductId, parsedPeriod);

            if (salesAggregate == null || !salesAggregate.Any())
                return Result<List<SalesAggregateDto>>.Failure("No history available", 404);

            var salesAggregateDto = mapper.Map<List<SalesAggregateDto>>(salesAggregate);

            return Result<List<SalesAggregateDto>>.Success(salesAggregateDto);
        }
    }
}
