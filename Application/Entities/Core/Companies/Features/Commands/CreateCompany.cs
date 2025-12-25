using Application.Core;
using Application.Entities.Core.Companies.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Companies.Features.Commands;

public class CreateCompany
{
    public class Command : IRequest<Result<CompanyDto>>
    {
        public required CreateCompanyDto CompanyDto { get; set; }
    }

    public class Handler(
        IRepository<Company> repository, 
        IMapper mapper
    ) : IRequestHandler<Command, Result<CompanyDto>>
    {
        public async Task<Result<CompanyDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CompanyDto;

            var company = mapper.Map<Company>(dto);

            await repository.AddAsync(company, ct);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<CompanyDto>.Failure("Failed to create business", 400);

            var createdCompanyDto = mapper.Map<CompanyDto>(company);

            return Result<CompanyDto>.Success(createdCompanyDto);
        }
    }
}
