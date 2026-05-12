using Domain.Entities.Core;
using Infrastructure.Repositories;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class RepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private readonly CompanyHelper _companyHelper = new CompanyHelper(fixture);
        private Repository<Company> CreateRepository() =>
            new Repository<Company>(Fixture.Context);

        [Fact]
        public async Task AddAsync_ValidEntity_PersistsToDatabase()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == company.Id, CancellationToken.None);

            Assert.NotNull(found);
            Assert.Equal(company.Name, found!.Name);
        }

        [Fact]
        public async Task ExistsAsync_ExistingEntity_ReturnsTrue()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var exists = await repo.ExistsAsync(
                c => c.Id == company.Id, 
                CancellationToken.None);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingEntity_ReturnsFalse()
        {
            var repo = CreateRepository();

            var exists = await repo.ExistsAsync(
                c => c.Id == Guid.NewGuid(), CancellationToken.None);

            Assert.False(exists);
        }

        [Fact]
        public async Task SingleOrDefaultAsync_ExistingEntity_ReturnsEntity()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == company.Id, CancellationToken.None);

            Assert.NotNull(found);
            Assert.Equal(company.Id, found!.Id);
        }

        [Fact]
        public async Task SingleOrDefaultAsync_NonExistingEntity_ReturnsNull()
        {
            var repo = CreateRepository();

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == Guid.NewGuid(), CancellationToken.None);

            Assert.Null(found);
        }

        [Fact]
        public async Task FindAsync_ExistingEntities_ReturnsEntities()
        {
            var repo = CreateRepository();
            var companies = await _companyHelper.CreateCompanies();

            var results = await repo.FindAsync(
                c => c.Name.Contains("Test Company"), 
                CancellationToken.None);

            Assert.NotEmpty(results);
            Assert.Equal(companies.Count, results.Count());
            Assert.All(results, r =>
                Assert.Contains(companies, c => c.Id == r.Id));
        }

        [Fact]
        public async Task FindAsync_NonExistingEntities_ReturnsEmpty()
        {
            var repo = CreateRepository();

            var results = await repo.FindAsync(
                c => c.Name.Contains("Test Company"), CancellationToken.None);

            Assert.Empty(results);
        }

        [Fact]
        public async Task Update_ExistingEntity_PersistsChanges()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            company.Name = "Updated Name";
            repo.Update(company);
            await repo.SaveChangesAsync(CancellationToken.None);

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == company.Id, CancellationToken.None);

            Assert.Equal("Updated Name", found!.Name);
        }

        [Fact]
        public async Task Remove_ExistingEntity_DeletesFromDatabase()
        {
            var repo = CreateRepository();
            var company = await _companyHelper.CreateCompany();

            repo.Remove(company);
            await repo.SaveChangesAsync(CancellationToken.None);

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == company.Id, CancellationToken.None);

            Assert.Null(found);
        }

        [Fact]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            var repo = CreateRepository();
            var companies = await _companyHelper.CreateCompanies();

            var count = await repo.CountAsync(
                c => c.Name.Contains("Test Company"),
                CancellationToken.None);

            Assert.Equal(companies.Count, count);
        }

        [Fact]
        public async Task CountAsync_NoMatches_ReturnsZero()
        {
            var repo = CreateRepository();

            var count = await repo.CountAsync(
                c => c.Name.Contains("DoesNotExist"),
                CancellationToken.None);

            Assert.Equal(0, count);
        }
    }
}