using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class CompanyRepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private CompanyRepository CreateRepository() =>
            new CompanyRepository(Fixture.Context);

        [Fact]
        public async Task GetCompanyIds_WhenExists_ReturnsCompanyIds()
        {
            var repo = CreateRepository();
            var companies = CompanyHelper.CreateCompanies();

            await Fixture.Context.Companies.AddRangeAsync(companies);
            await Fixture.Context.SaveChangesAsync();

            var expected = companies
                .Select(x => x.Id)
                .OrderBy(x => x)
                .ToList();

            var result = await repo.GetCompanyIds(CancellationToken.None);
            result = result.OrderBy(x => x).ToList();

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetCompanyIds_WhenNotExists_ReturnsEmptyList()
        {
            var repo = CreateRepository();
            var result = await repo.GetCompanyIds(CancellationToken.None);

            Assert.Empty(result);
        }
    }
}
