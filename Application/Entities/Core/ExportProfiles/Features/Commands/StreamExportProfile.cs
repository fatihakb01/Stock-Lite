using Application.Core;
using Domain.Entities.Core;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Export;
using Domain.Interfaces.Services.Export.Records;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Commands
{
    public class StreamExportProfile
    {
        public class Command : IRequest<Result<ExportResult>>
        {
            public Guid Id { get; set; }
            public FileType FileType { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            IExportService service,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<ExportResult>>
        {
            public async Task<Result<ExportResult>> Handle(Command request, CancellationToken ct)
            {
                var exportProfile = await repository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (exportProfile == null)
                    return Result<ExportResult>.Failure("ExportProfile not found", 404);

                var exportResult = await service.ExportToStreamAsync(exportProfile, request.FileType, currentUser.CompanyId, ct);

                return Result<ExportResult>.Success(exportResult);
            }
        }
    }
}
