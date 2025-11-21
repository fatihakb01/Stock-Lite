using Application.Core;
using Application.Entities.Core.Customers.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Customers.Features.Commands;

public class EditCustomer
{
    public class Command : IRequest<Result<EditCustomerDto>>
    {
        public Guid Id { get; set; }
        public required EditCustomerDto CustomerDto { get; set; }
    }

    public class Handler(IRepository<Customer> repository, IMapper mapper)
        : IRequestHandler<Command, Result<EditCustomerDto>>
    {
        public async Task<Result<EditCustomerDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
                return Result<EditCustomerDto>.Failure("Customer not found", 404);

            mapper.Map(request.CustomerDto, customer);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<EditCustomerDto>.Failure("Failed to update customer", 400);

            var updatedCustomerDto = mapper.Map<EditCustomerDto>(customer);

            return Result<EditCustomerDto>.Success(updatedCustomerDto);
        }
    }
}
