using Application.Core;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Products.Features.Commands;

public class DeleteProduct
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
        // public Guid CompanyId { get; set; } // not yet implemented
    }

    public class Handler(IRepository<Product> repository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
                return Result<Unit>.Failure("Product not found", 404);

            repository.Remove(product);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Faild to delete product", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
