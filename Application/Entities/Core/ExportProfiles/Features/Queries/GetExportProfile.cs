using Application.Core;
using Application.Entities.Core.ExportProfiles.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
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
            IMapper mapper,
            ICurrentUserService currentUser
        ) : IRequestHandler<Query, Result<ExportProfileDetailDto>>
        {
            public async Task<Result<ExportProfileDetailDto>> Handle(Query request, CancellationToken ct)
            {
                var exportProfile = await repository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (exportProfile == null)
                    return Result<ExportProfileDetailDto>.Failure("ExportProfile not found", 404);

                var exportProfileDto = mapper.Map<ExportProfileDetailDto>(exportProfile);

                return Result<ExportProfileDetailDto>.Success(exportProfileDto);
            }
        }
    }
}
