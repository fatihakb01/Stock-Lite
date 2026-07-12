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
                var cacheKey = $"productkpis:{currentUser.CompanyId}";
                var cached = await cacheService.GetAsync<ProductKpiDto>(cacheKey, ct);

                if (cached != null)
                {
                    dto.Status = cached.SchedulerStatus;
                    return Result<SchedulerStatusDto>.Success(dto);
                }

                var schedulerStatus = await repository.GetSchedulerStatus(currentUser.CompanyId, ct);
                dto.Status = schedulerStatus;

                await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), ct);

                return Result<SchedulerStatusDto>.Success(dto);
            }
        }
    }
}
