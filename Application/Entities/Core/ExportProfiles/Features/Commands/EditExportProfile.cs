using Application.Core;
using Application.Entities.Core.ExportProfiles.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Commands
{
    public class EditExportProfile
    {
        public class Command : IRequest<Result<ExportProfileDetailDto>>
        {
            public Guid Id { get; set; }
            public required EditExportProfileDto Dto { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            IBaseCacheService<ExportProfile> cacheService,
            ICurrentUserService currentUser,
            IMapper mapper
        ) : IRequestHandler<Command, Result<ExportProfileDetailDto>>
        {
            public async Task<Result<ExportProfileDetailDto>> Handle(Command request, CancellationToken ct)
            {
                var dto = request.Dto;
                var exportProfile = await repository.SingleOrDefaultAsync(ex =>
                    ex.Id == request.Id && ex.CompanyId == currentUser.CompanyId,
                    ct
                );
                if (exportProfile == null)
                    return Result<ExportProfileDetailDto>.Failure("ExportProfile not found", 404);

                var nameExist = await repository.ExistsAsync(ex => 
                    ex.Name == dto.Name && ex.Id != request.Id, 
                    ct
                );
                if (nameExist)
                    return Result<ExportProfileDetailDto>.Failure("Name must be unique", 400);

                mapper.Map(dto, exportProfile);
                var result = await repository.SaveChangesAsync(ct);
                if (!result)
                    return Result<ExportProfileDetailDto>.Failure("Failed to update ExportProfile", 400);

                var updatedExportProfile = mapper.Map<ExportProfileDetailDto>(exportProfile);
                await cacheService.RemoveKeysAsync(exportProfile.CompanyId, [updatedExportProfile.Id], null, ct);
                return Result<ExportProfileDetailDto>.Success(updatedExportProfile);
            }
        }
    }
}
