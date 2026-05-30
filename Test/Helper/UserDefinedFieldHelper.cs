using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class UserDefinedFieldHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal async Task<UserDefinedField> CreateUserDefinedField(
            Product product,
            string key = "AdjustedStock",
            string? value = "10",
            bool saveChanges = true)
        {
            var field = new UserDefinedField
            {
                ProductId = product.Id,
                Product = product,
                CompanyId = product.CompanyId,
                Key = key,
                Value = value
            };

            await _fixture.Context.UserDefinedFields.AddAsync(field);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return field;
        }

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

        internal async Task<UserDefinedField?> GetField(
            Guid companyId,
            string sku,
            string key)
        {
            return await _fixture.Context.UserDefinedFields
                .AsNoTracking()
                .Include(x => x.Product)
                .SingleOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.Product.SKU == sku &&
                    x.Key == key);
        }
    }
}
