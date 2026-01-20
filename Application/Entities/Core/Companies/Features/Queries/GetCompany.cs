using Application.Core;
using Application.Entities.Core.Companies.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.Companies.Features.Queries;

public class GetCompany
{
    public class Query : IRequest<Result<CompanyDto>>
    {
        
    }

    public class Handler(
        IRepository<Company> repository, 
        IMapper mapper,
        ICurrentUserService currentUser
    )
        : IRequestHandler<Query, Result<CompanyDto>>
    {
        public async Task<Result<CompanyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var company = await repository.GetByIdAsync(currentUser.CompanyId, cancellationToken);

            if (company == null)
                return Result<CompanyDto>.Failure("Company not found", 404);

            var companyDto = mapper.Map<CompanyDto>(company);

            return Result<CompanyDto>.Success(companyDto);
        }
    }
}
