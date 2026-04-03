using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.ExportProfiles.Features.Commands
{
    public class DeleteExportProfile
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler(
            IRepository<ExportProfile> repository,
            ICurrentUserService currentUser
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var exportProfile = await repository.SingleOrDefaultAsync(x =>
                    x.Id == request.Id && x.CompanyId == currentUser.CompanyId,
                    ct
                );

                if (exportProfile == null)
                    return Result<Unit>.Failure("ExportProfile not found", 404);

                repository.Remove(exportProfile);

                var result = await repository.SaveChangesAsync(ct);

                if (!result)
                    return Result<Unit>.Failure("Failed to delete ExportProfile", 400);

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
