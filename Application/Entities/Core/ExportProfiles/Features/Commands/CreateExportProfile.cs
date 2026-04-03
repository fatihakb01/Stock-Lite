using Application.Core;
using Application.Entities.Core.ExportProfiles.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Commands
{
    public class CreateExportProfile
    {
        public class Command : IRequest<Result<ExportProfileDetailDto>>
        {
            public required CreateExportProfileDto Dto { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            ICurrentUserService currentUser,
            IMapper mapper
        ) : IRequestHandler<Command, Result<ExportProfileDetailDto>>
        {
            public async Task<Result<ExportProfileDetailDto>> Handle(Command request, CancellationToken ct)
            {
                var dto = request.Dto;

                var nameExist = await repository.ExistsAsync(ex => ex.Name == dto.Name, ct);
                if (nameExist)
                    return Result<ExportProfileDetailDto>.Failure("Name must be unique", 400);

                var exportProfile = mapper.Map<ExportProfile>(dto);
                exportProfile.CreatedBy = currentUser.Id;
                exportProfile.CompanyId = currentUser.CompanyId;

                await repository.AddAsync(exportProfile,ct);

                var result = await repository.SaveChangesAsync(ct);
                if (!result)
                    return Result<ExportProfileDetailDto>.Failure("Failed to save ExportProfile", 400);

                var createdExportProfile = mapper.Map<ExportProfileDetailDto>(exportProfile);

                return Result<ExportProfileDetailDto>.Success(createdExportProfile);
            }
        }
    }
}
