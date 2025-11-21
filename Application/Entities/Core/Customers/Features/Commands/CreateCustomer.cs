using Application.Core;
using Application.Entities.Core.Customers.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Customers.Features.Commands;

public class CreateCustomer
{
    public class Command : IRequest<Result<CustomerDto>>
    {
        public required CreateCustomerDto CustomerDto { get; set; }
    }

    public class Handler(IRepository<Customer> repository, IMapper mapper)
        : IRequestHandler<Command, Result<CustomerDto>>
    {
        public async Task<Result<CustomerDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var customer = mapper.Map<Customer>(request.CustomerDto);

            await repository.AddAsync(customer, cancellationToken);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<CustomerDto>.Failure("Failed to create customer", 400);

            var createdCustomerDto = mapper.Map<CustomerDto>(customer);

            return Result<CustomerDto>.Success(createdCustomerDto);
        }
    }
}
