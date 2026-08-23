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
    public class GetExportProfiles
    {
        public class Query : IRequest<Result<List<ExportProfileDto>>>
        {

        }

        public class Handler(
            IRepository<ExportProfile> repository,
            IBaseCacheService<ExportProfile> cacheService,
            IMapper mapper,
            ICurrentUserService currentUser
        ) : IRequestHandler<Query, Result<List<ExportProfileDto>>>
        {
            public async Task<Result<List<ExportProfileDto>>> Handle(Query request, CancellationToken ct)
            {
                var cacheKey = cacheService.GetKey(currentUser.CompanyId, ExportProfileKey.ExportProfiles);
                var cached = await cacheService.GetAsync<List<ExportProfileDto>>(cacheKey, ct);
                if (cached != null)
                    return Result<List<ExportProfileDto>>.Success(cached);

                var exportProfiles = await repository.FindAsync(x =>
                    x.CompanyId == currentUser.CompanyId,
                    ct
                );
                if (!exportProfiles.Any())
                    return Result<List<ExportProfileDto>>.Failure("ExportProfiles not found", 404);

                var exportProfileDtos = mapper.Map<List<ExportProfileDto>>(exportProfiles);
                await cacheService.SetAsync(cacheKey, exportProfileDtos, TimeSpan.FromMinutes(5), ct);
                return Result<List<ExportProfileDto>>.Success(exportProfileDtos);
            }
        }
    }
}
