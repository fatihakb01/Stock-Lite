using Application.Core;
using Application.Entities.Core.UserDefinedFields.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Users;
using MediatR;

namespace Application.Entities.Core.UserDefinedFields.Features.Queries;

public class GetUserDefinedFieldsByProductId
{
    public class Query : IRequest<Result<List<UserDefinedFieldDto>>>
    {
        public Guid ProductId { get; set; }
    }

    public class Handler(
        IRepository<UserDefinedField> repository,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<List<UserDefinedFieldDto>>>
    {
        public async Task<Result<List<UserDefinedFieldDto>>> Handle(Query request, CancellationToken ct)
        {
            var udfs = await repository.FindAsync(x => 
                x.ProductId == request.ProductId &&
                x.Product.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!udfs.Any())
                return Result<List<UserDefinedFieldDto>>.Failure("User Defined Fields not found", 404);

            var udfDtos = mapper.Map<List<UserDefinedFieldDto>>(udfs);

            return Result<List<UserDefinedFieldDto>>.Success(udfDtos);
        }
    }
}
