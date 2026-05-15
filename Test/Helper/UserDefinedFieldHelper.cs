using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class UserDefinedFieldHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal async Task<int> CountUserDefinedFields(Guid companyId)
        {
            return await _fixture.Context.UserDefinedFields
                .CountAsync(x => x.Product.CompanyId == companyId);
        }
    }
}
