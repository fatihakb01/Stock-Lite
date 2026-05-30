using Domain.Entities.Core;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class CompanyHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal async Task<Company> CreateCompany(
            string name = "Test Company",
            string email = "test@test.com",
            bool isActive = true,
            bool saveChanges = true
        )
        {
            var company = CreateCompanyStatic(name, email, isActive);

            await _fixture.Context.Companies.AddAsync(company);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return company;
        }

        internal static Company CreateCompanyStatic(
            string name = "Test Company",
            string email = "test@test.com",
            bool isActive = true
        )
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            return company;
        }

        internal async Task<List<Company>> CreateCompanies(string name = "Test Company", int total = 10)
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

            await _fixture.Context.Companies.AddRangeAsync(companies);
            await _fixture.Context.SaveChangesAsync();

            return companies;
        }
    }
}
