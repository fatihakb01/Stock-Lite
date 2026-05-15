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

        private static string BuildCsvHeaders()
        {
            return "SKU;Key;Value\r\n";
        }

        internal static string BuildCsv(
            string sku = "SKU-0001",
            string key = "AdjustedStock",
            string value = "10",
            bool includeHeaders = true
            )
        {
            string content = $"{sku};{key};{value}";
            if (includeHeaders)
            {
                return BuildCsvHeaders() + $"{content}\r\n";
            }

            return $"{content}\r\n";
        }
    }
}
