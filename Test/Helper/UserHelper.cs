using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resend;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class UserHelper(DatabaseFixture fixture, Mock<IResend> resendMock, Mock<IWebHostEnvironment> envMock)
    {
        private readonly DatabaseFixture _fixture = fixture;
        private readonly Mock<IResend> _resendMock = resendMock;
        private readonly Mock<IWebHostEnvironment> _envMock = envMock;

        internal UserRepository CreateRepository()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDataProtection();
            services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DataContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddDefaultTokenProviders();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(_resendMock.Object);
            services.AddSingleton(_envMock.Object);
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(_fixture.ConnectionString));

            var provider = services.BuildServiceProvider();
            var userManager = provider.GetRequiredService<UserManager<User>>();

            return new UserRepository(
                userManager,
                _resendMock.Object,
                configuration,
                _envMock.Object
            );
        }

        internal static User CreateUser(
            Company company,
            string email = "test@test.com",
            string userName = "testuser")
        {
            return new User
            {
                Email = email,
                UserName = userName,
                EmailConfirmed = false,
                Role = UserRole.User,
                Company = company,
                CompanyId = company.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        internal async Task<User> CreateDummyUserAsync(
            Company company,
            string email = "test@test.com",
            string userName = "testuser")
        {
            var user = CreateUser(company, email, userName);
            user.PasswordHash = "dummy-hash";

            _fixture.Context.Users.Add(user);
            await _fixture.Context.SaveChangesAsync();

            return user;
        }
    }
}
