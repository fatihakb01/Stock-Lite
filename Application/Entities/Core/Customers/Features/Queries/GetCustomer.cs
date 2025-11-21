using Application.Core;
using Application.Entities.Core.Customers.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Customers.Features.Queries;

public class GetCustomer
{
    public class Query : IRequest<Result<CustomerDto>>
    {
        public required Guid Id { get; set; }
    }

    public class Handler(IRepository<Customer> repository, IMapper mapper)
        : IRequestHandler<Query, Result<CustomerDto>>
    {
        public async Task<Result<CustomerDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
                return Result<CustomerDto>.Failure("Customer not found", 404);

            var customerDto = mapper.Map<CustomerDto>(customer);

            return Result<CustomerDto>.Success(customerDto);
        }
    }
}
