using Application.Core;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Customers.Features.Commands;

public class DeleteCustomer
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IRepository<Customer> repository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
                return Result<Unit>.Failure("Customer not found", 404);

            repository.Remove(customer);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<Unit>.Failure("Failed to delete customer", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
