using Domain.Entities.Core;
using Domain.Enums;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class ExportProfileHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal static ExportProfile CreateStandardProfileStatic(
            ExportType exportType = ExportType.StockLevels,
            string? targetPath = null,
            string? customSource = null,
            CustomType? type = null,
            string name = "Test Export",
            Company? company = null)
            => new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                ExportType = exportType,
                CustomSource = customSource,
                CustomType = type,
                TargetPath = targetPath,
                CompanyId =  company != null ? company.Id : Guid.NewGuid(),
                Company = company ?? null!
            };

        internal async Task<ExportProfile> CreateStandardProfile(
            ExportType exportType = ExportType.StockLevels,
            string? targetPath = null,
            string name = "Test Export",
            Company? company = null
        )
        {
            var profile = CreateStandardProfileStatic(
                exportType: exportType,
                targetPath: targetPath,
                name: name,
                company: company
            );

            await _fixture.Context.ExportProfile.AddAsync(profile);
            await _fixture.Context.SaveChangesAsync();

            return profile;
        }

        internal static ExportProfile CreateCustomProfileStatic(
            string customSource = "dbo.custom_test_export",
            CustomType customType = CustomType.View,
            string? targetPath = null,
            Company? company = null)
            => new()
            {
                Id = Guid.NewGuid(),
                Name = "Custom Export",
                ExportType = null,
                CustomSource = customSource,
                CustomType = customType,
                TargetPath = targetPath,
                CompanyId = company != null ? company.Id : Guid.NewGuid(),
                Company = company ?? null!
            };

        internal async Task<ExportProfile> CreateCustomProfile(
            string customSource = "dbo.custom_test_export",
            CustomType customType = CustomType.View,
            string? targetPath = null,
            Company? company = null
        )
        {
            var profile = CreateCustomProfileStatic(
                customSource: customSource,
                customType: customType,
                targetPath: targetPath,
                company: company
            );

            await _fixture.Context.ExportProfile.AddAsync(profile);
            await _fixture.Context.SaveChangesAsync();

            return profile;
        }
    }
}
