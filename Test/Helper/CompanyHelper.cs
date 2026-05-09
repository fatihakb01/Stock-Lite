using Domain.Entities.Core;

namespace Test.Helper
{
    internal static class CompanyHelper
    {
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
    }
}
