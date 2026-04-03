using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Export;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Commands
{
    public class RunExportProfile
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            IExportService service,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var exportProfile = await repository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (exportProfile == null)
                    return Result<Unit>.Failure("ExportProfile not found", 404);

                if (exportProfile.TargetPath != null)
                    await service.ExportAsync(exportProfile, currentUser.CompanyId, ct);
                else
                    return Result<Unit>.Failure("TargetPath must be filled in", 400);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
