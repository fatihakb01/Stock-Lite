using Application.Core;
using Application.Entities.Core.ExportProfiles.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Queries
{
    public class GetExportProfile
    {
        public class Query : IRequest<Result<ExportProfileDetailDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            IBaseCacheService<ExportProfile> cacheService,
            IMapper mapper,
            ICurrentUserService currentUser
        ) : IRequestHandler<Query, Result<ExportProfileDetailDto>>
        {
            public async Task<Result<ExportProfileDetailDto>> Handle(Query request, CancellationToken ct)
            {
                var cacheKey = cacheService.GetKey(request.Id, ExportProfileKey.ExportProfile);
                var cached = await cacheService.GetAsync<ExportProfileDetailDto>(cacheKey, ct);
                if (cached != null)
                    return Result<ExportProfileDetailDto>.Success(cached);

                var exportProfile = await repository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                    ct
                );
                if (exportProfile == null)
                    return Result<ExportProfileDetailDto>.Failure("ExportProfile not found", 404);

                var exportProfileDto = mapper.Map<ExportProfileDetailDto>(exportProfile);
                await cacheService.SetAsync(cacheKey, exportProfileDto, TimeSpan.FromMinutes(5), ct);
                return Result<ExportProfileDetailDto>.Success(exportProfileDto);
            }
        }
    }
}
