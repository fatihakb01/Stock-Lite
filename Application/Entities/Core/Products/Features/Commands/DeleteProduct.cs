using Application.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Products.Features.Commands;

public class DeleteProduct
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(
        IProductRepository repository, 
        ICurrentUserService currentUser
    )
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var product = await repository.SingleOrDefaultAsync(x => 
                x.Id == request.Id && x.CompanyId == currentUser.CompanyId, 
                ct
            );

            if (product == null)
                return Result<Unit>.Failure("Product not found", 404);

            repository.Remove(product);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<Unit>.Failure("Failed to delete product", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
