using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Resend;
using Test.Helper;
using Test.Integration.Helper;

namespace Test.Integration.Repositories
{
    public class UserRepositoryTest
        : IntegrationTestBase, IClassFixture<DatabaseFixture>
    {
        private readonly Mock<IResend> _resendMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly UserHelper _userHelper;

        public UserRepositoryTest(DatabaseFixture fixture)
            : base(fixture)
        {
            _resendMock = new Mock<IResend>();
            _envMock = new Mock<IWebHostEnvironment>();

            _userHelper = new UserHelper(
                fixture,
                _resendMock,
                _envMock
            );
        }

        [Fact]
        public async Task CreateAsync_WhenValid_CreatesUser()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            var result = await repo.CreateAsync(user, "Password123!");

            Assert.True(result.Succeeded);

            var createdUser = await Fixture.Context.Users.FindAsync(user.Id);

            Assert.NotNull(createdUser);
            Assert.Equal(user.Email, createdUser!.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenExists_ReturnsUser()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var result = await repo.GetByEmailAsync(user.Email!);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenMissing_ReturnsNull()
        {
            var repo = _userHelper.CreateRepository();

            var result = await repo.GetByEmailAsync("missing@test.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsUser()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);
            var userId = Guid.Parse(user.Id);

            await repo.CreateAsync(user, "Password123!");

            var result = await repo.GetByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            var repo = _userHelper.CreateRepository();

            var result = await repo.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenCorrect_ReturnsTrue()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var result = await repo.CheckPasswordAsync(user, "Password123!");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenIncorrect_ReturnsFalse()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var result = await repo.CheckPasswordAsync(user, "WrongPassword!");

            Assert.False(result);
        }

        [Fact]
        public async Task GenerateEmailConfirmationTokenAsync_ReturnsToken()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            var token = await repo.GenerateEmailConfirmationTokenAsync(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenValidToken_ConfirmsEmail()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var token = await repo.GenerateEmailConfirmationTokenAsync(user);
            var result = await repo.ConfirmEmailAsync(user, token);

            Assert.True(result.Succeeded);

            var updatedUser = await Fixture.Context.Users.FindAsync(user.Id);

            Assert.True(updatedUser!.EmailConfirmed);
        }

        [Fact]
        public async Task GeneratePasswordResetTokenAsync_ReturnsToken()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var token = await repo.GeneratePasswordResetTokenAsync(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenValidToken_UpdatesPassword()
        {
            var repo = _userHelper.CreateRepository();
            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");

            var token = await repo.GeneratePasswordResetTokenAsync(user);
            var result = await repo.ResetPasswordAsync(
                user,
                token,
                "NewPassword123!");

            Assert.True(result.Succeeded);

            var passwordValid = await repo.CheckPasswordAsync(
                user,
                "NewPassword123!");

            Assert.True(passwordValid);
        }

        [Fact]
        public async Task SendConfirmationEmailAsync_WhenDevelopment_SendsToConfiguredEmail()
        {
            var repo = _userHelper.CreateRepository();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            _envMock.Setup(x => x.EnvironmentName)
                .Returns(Environments.Development);

            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company);

            await repo.CreateAsync(user, "Password123!");
            await repo.SendConfirmationEmailAsync(
                user,
                "https://localhost/confirm");

            _resendMock.Verify(
                x => x.EmailSendAsync(
                    It.Is<EmailMessage>(m =>
                        m.Subject == "Confirm your email address" &&
                        m.To.Contains(config["Resend:Email:To"]!)
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_WhenProduction_SendsToActualUser()
        {
            var repo = _userHelper.CreateRepository();

            _envMock
                .Setup(x => x.EnvironmentName)
                .Returns(Environments.Production);

            var company = CompanyHelper.CreateCompany();
            var user = UserHelper.CreateUser(company: company, email: "realuser@test.com");
            var userId = Guid.Parse(user.Id);

            await repo.CreateAsync(user, "Password123!");
            await repo.SendPasswordResetEmailAsync(
                user,
                "https://localhost/reset");

            _resendMock.Verify(
                x => x.EmailSendAsync(
                    It.Is<EmailMessage>(m =>
                        m.Subject == "Reset your password" &&
                        m.To.Contains("realuser@test.com")
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}