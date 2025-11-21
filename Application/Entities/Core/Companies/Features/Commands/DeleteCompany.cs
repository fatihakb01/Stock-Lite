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

    public class Handler(IRepository<Company> repository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var company = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (company == null)
                return Result<Unit>.Failure("Company not found", 404);

            repository.Remove(company);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Failed to delete company", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
