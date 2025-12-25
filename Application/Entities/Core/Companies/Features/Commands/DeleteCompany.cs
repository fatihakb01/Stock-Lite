using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Companies.Features.Commands;

public class DeleteCompany
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IRepository<Company> repository,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var companies = await repository.FindAsync(x => 
                x.Id == currentUser.CompanyId, 
                ct
            );

            var company = companies.SingleOrDefault();

            if (company == null)
                return Result<Unit>.Failure("Company not found", 404);

            repository.Remove(company);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<Unit>.Failure("Failed to delete company", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
