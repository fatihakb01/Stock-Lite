using Application.Core;
using Application.Entities.Derived.Seasonality.DTOs;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.Seasonality.Features.Queries;

public class GetSeasonality
{
    public class Query : IRequest<Result<List<SeasonalityDto>>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        ISeasonalityRepository seasonalityRepo,
        IProductRepository productRepo,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<List<SeasonalityDto>>>
    {
        public async Task<Result<List<SeasonalityDto>>> Handle(Query request, CancellationToken ct)
        {
            var products = await productRepo.ExistsAsync(x =>
                x.Id == request.ProductId && 
                x.CompanyId == currentUser.CompanyId,
                ct
            );
            
            if (!products)
                return Result<List<SeasonalityDto>>.Failure($"Product not found for this company", 404);

            var seasonality = await seasonalityRepo.GetSeasonalityByProductAsync(request.ProductId);

            if (seasonality == null)
                return Result<List<SeasonalityDto>>.Failure($"No seasonality available", 404);

            var seasonalityDto = mapper.Map<List<SeasonalityDto>>(seasonality);

            return Result<List<SeasonalityDto>>.Success(seasonalityDto);
        }
    }
}
