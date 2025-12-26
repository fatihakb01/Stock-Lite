using Application.Core;
using Application.Entities.Core.Companies.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces;
using MediatR;

namespace Application.Entities.Core.Companies.Features.Commands;

public class EditCompany
{
    public class Command : IRequest<Result<CompanyDto>>
    {
        public required EditCompanyDto CompanyDto { get; set; }
    }

    public class Handler(
        IRepository<Company> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    ) : IRequestHandler<Command, Result<CompanyDto>>
    {
        public async Task<Result<CompanyDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.CompanyDto;

            var companies = await repository.FindAsync(x =>
                x.Id == currentUser.CompanyId, 
                ct
            );

            var company = companies.SingleOrDefault();

            if (company == null)
                return Result<CompanyDto>.Failure("Company not found", 404);

            mapper.Map(request.CompanyDto, company);

            var result = await repository.SaveChangesAsync(ct);

            if (!result)
                return Result<CompanyDto>.Failure("Failed to update company", 400);

            var updatedCompanyDto = mapper.Map<CompanyDto>(company);

            return Result<CompanyDto>.Success(updatedCompanyDto);
        }
    }
}
