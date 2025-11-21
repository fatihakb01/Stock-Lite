using Application.Core;
using Application.Entities.Core.Companies.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Companies.Features.Commands;

public class EditCompany
{
    public class Command : IRequest<Result<EditCompanyDto>>
    {
        public Guid Id { get; set; }
        public required EditCompanyDto CompanyDto { get; set; }
    }

    public class Handler(IRepository<Company> repository, IMapper mapper)
        : IRequestHandler<Command, Result<EditCompanyDto>>
    {
        public async Task<Result<EditCompanyDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var company = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (company == null)
                return Result<EditCompanyDto>.Failure("Company not found", 404);

            mapper.Map(request.CompanyDto, company);

            var result = await repository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Result<EditCompanyDto>.Failure("Failed to update company", 400);

            var updatedCompanyDto = mapper.Map<EditCompanyDto>(company);

            return Result<EditCompanyDto>.Success(updatedCompanyDto);
        }
    }
}
