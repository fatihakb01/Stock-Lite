using Application.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Derived.ProductKpis.Features.Queries
{
    public class GetSchedulerStatus
    {
        public class Query : IRequest<Result<JobExecutionStatus>> { }

        public class Handler(
            IProductKpiRepository repository,
            ICurrentUserService currentUser
        ) : IRequestHandler<Query, Result<JobExecutionStatus>>
        {
            public async Task<Result<JobExecutionStatus>> Handle(Query request, CancellationToken ct)
            {
                var schedulerStatus = await repository.GetSchedulerStatus(currentUser.CompanyId, ct);

                return Result<JobExecutionStatus>.Success(schedulerStatus);
            }
        }
    }
}
