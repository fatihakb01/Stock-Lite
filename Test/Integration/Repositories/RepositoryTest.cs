using Domain.Entities.Core;
using Infrastructure.Repositories;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class RepositoryTest(DatabaseFixture fixture)
        : IntegrationTestBase(fixture), IClassFixture<DatabaseFixture>
    {
        private Repository<Company> CreateRepository() =>
            new Repository<Company>(Fixture.Context);

        internal static Company CreateCompany(string name = "Test Company") => new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = "test@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        internal static List<Company> CreateCompanies(string name = "Test Company", int total = 10)
        {
            List<Company> companies = new List<Company>();

            for (int i = 1; i <= total; i++)
            {
                companies.Add(new Company
                {
                    Id = Guid.NewGuid(),
                    Name = name + " " + i,
                    Email = "test" + i + "@test.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return companies;
        }

        [Fact]
        public async Task AddAsync_ValidEntity_PersistsToDatabase()
        {
            var repo = CreateRepository();
            var company = CreateCompany();

            await repo.AddAsync(company, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            var found = await repo.SingleOrDefaultAsync(
                c => c.Id == company.Id, CancellationToken.None);

            Assert.NotNull(found);
            Assert.Equal(company.Name, found!.Name);
        }

        [Fact]
        public async Task ExistsAsync_ExistingEntity_ReturnsTrue()
        {
            var repo = CreateRepository();
            var company = CreateCompany();

            await repo.AddAsync(company, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

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
            var company = CreateCompany();

            await repo.AddAsync(company, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

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
            var companies = CreateCompanies();

            foreach (var company in companies)
                await repo.AddAsync(company, CancellationToken.None);
            
            await repo.SaveChangesAsync(CancellationToken.None);

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
            var company = CreateCompany();

            await repo.AddAsync(company, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

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
            var company = CreateCompany();

            await repo.AddAsync(company, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

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
            var companies = CreateCompanies();

            foreach (var c in companies)
                await repo.AddAsync(c, CancellationToken.None);

            await repo.SaveChangesAsync(CancellationToken.None);

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