using Application.Core;
using Application.Entities.Core.UserDefinedFields.DTOs;
using AutoMapper;
using Domain.Entities.Core;
using Domain.Enums.Keys;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services.Redis;
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
        IBaseCacheService<UserDefinedField> cacheService,
        ICurrentUserService currentUser,
        IMapper mapper
    ) : IRequestHandler<Query, Result<List<UserDefinedFieldDto>>>
    {
        public async Task<Result<List<UserDefinedFieldDto>>> Handle(Query request, CancellationToken ct)
        {
            var cacheKey = cacheService.GetKey(request.ProductId, UserDefinedFieldKey.UserDefinedField);
            var cached = await cacheService.GetAsync<List<UserDefinedFieldDto>>(cacheKey, ct);
            if (cached != null)
                return Result<List<UserDefinedFieldDto>>.Success(cached);

            var udfs = await repository.FindAsync(x => 
                x.ProductId == request.ProductId &&
                x.CompanyId == currentUser.CompanyId,
                ct
            );

            if (!udfs.Any())
                return Result<List<UserDefinedFieldDto>>.Failure("User Defined Fields not found", 404);

            var udfDtos = mapper.Map<List<UserDefinedFieldDto>>(udfs);

            await cacheService.SetAsync(cacheKey, udfDtos, TimeSpan.FromMinutes(5), ct);

            return Result<List<UserDefinedFieldDto>>.Success(udfDtos);
        }
    }
}
