using Application.Core;
using Application.Entities.Derived.ProductKpis.DTOs;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ProductKpis.Features.Queries
{
    public class GetSchedulerStatus
    {
        public class Query : IRequest<Result<SchedulerStatusDto>> { }

        public class Handler(
            IProductKpiRepository repository,
            ICacheService cacheService,
            ICurrentUserService currentUser
        ) : IRequestHandler<Query, Result<SchedulerStatusDto>>
        {
            public async Task<Result<SchedulerStatusDto>> Handle(Query request, CancellationToken ct)
            {
                var dto = new SchedulerStatusDto { Status = JobExecutionStatus.Pending };
                var cacheKey = $"productkpis-schedulerstatus:{currentUser.CompanyId}";
                var cached = await cacheService.GetAsync<SchedulerStatusDto>(cacheKey, ct);
                if (cached != null)
                    return Result<SchedulerStatusDto>.Success(cached);

                var schedulerStatus = await repository.GetSchedulerStatus(currentUser.CompanyId, ct);
                dto.Status = schedulerStatus;
                await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), ct);

                return Result<SchedulerStatusDto>.Success(dto);
            }
        }
    }
}
