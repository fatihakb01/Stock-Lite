using Domain.Entities.Core;
using Domain.Enums;
using Infrastructure.Services.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Test.Unit.Users
{
    public class TokenServiceTest
    {
        private const string Key = "super-secret-test-key-that-is-long-enough-32chars";
        private const string Issuer = "TestIssuer";
        private const string Audience = "TestAudience";
        private const string ExpiryMin = "60";

        private static TokenService CreateService()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key",           Key       },
                { "Jwt:Issuer",        Issuer    },
                { "Jwt:Audience",      Audience  },
                { "Jwt:ExpireMinutes", ExpiryMin }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            return new TokenService(config);
        }

        private static User CreateUser() => new User
        {
            UserName = "testuser",
            Email = "test@test.com",
            Role = UserRole.Admin,
            CompanyId = Guid.NewGuid(),
            Company = null!,
            IsActive = true
        };

        private static JwtSecurityToken ParseToken(string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(tokenString);
        }

        private ClaimsPrincipal ValidateToken(string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            return handler.ValidateToken(tokenString, parameters, out _);
        }

        // -------------------------------------------------------
        // Token structure
        // -------------------------------------------------------
        [Fact]
        public void CreateToken_ReturnsNonEmptyString()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = service.CreateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void CreateToken_ReturnsValidJwtFormat()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = service.CreateToken(user);

            // A JWT always has exactly two dots separating three base64 segments
            Assert.Equal(2, token.Count(c => c == '.'));
        }

        [Fact]
        public void CreateToken_IsSignedWithCorrectKey()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = service.CreateToken(user);

            // ValidateToken throws if signature is invalid
            var principal = ValidateToken(token);
            Assert.NotNull(principal);
        }

        // -------------------------------------------------------
        // Claims
        // -------------------------------------------------------
        [Fact]
        public void CreateToken_ContainsNameIdentifierClaim()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c => 
                   c.Type == JwtRegisteredClaimNames.Sub
                || c.Type == ClaimTypes.NameIdentifier
                || c.Type == "nameid");

            Assert.NotNull(claim);
            Assert.Equal(user.Id.ToString(), claim!.Value);
        }

        [Fact]
        public void CreateToken_ContainsEmailClaim()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email || c.Type == "email");

            Assert.NotNull(claim);
            Assert.Equal(user.Email, claim!.Value);
        }

        [Fact]
        public void CreateToken_ContainsUserNameClaim()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Name || c.Type == "unique_name");

            Assert.NotNull(claim);
            Assert.Equal(user.UserName, claim!.Value);
        }

        [Fact]
        public void CreateToken_ContainsRoleClaim()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role || c.Type == "role");

            Assert.NotNull(claim);
            Assert.Equal(user.Role.ToString(), claim!.Value);
        }

        [Fact]
        public void CreateToken_ContainsCompanyIdClaim()
        {
            var service = CreateService();
            var user = CreateUser();

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c => c.Type == "companyId");

            Assert.NotNull(claim);
            Assert.Equal(user.CompanyId.ToString(), claim!.Value);
        }

        // -------------------------------------------------------
        // Issuer and Audience
        // -------------------------------------------------------
        [Fact]
        public void CreateToken_HasCorrectIssuer()
        {
            var service = CreateService();
            var token = ParseToken(service.CreateToken(CreateUser()));

            Assert.Equal(Issuer, token.Issuer);
        }

        [Fact]
        public void CreateToken_HasCorrectAudience()
        {
            var service = CreateService();
            var token = ParseToken(service.CreateToken(CreateUser()));

            Assert.Equal(Audience, token.Audiences.First());
        }

        // -------------------------------------------------------
        // Expiry
        // -------------------------------------------------------
        [Fact]
        public void CreateToken_ExpiresApproximately60MinutesFromNow()
        {
            var service = CreateService();
            var token = ParseToken(service.CreateToken(CreateUser()));

            var before = DateTime.UtcNow.AddMinutes(59);
            var after = DateTime.UtcNow.AddMinutes(61);

            Assert.InRange(token.ValidTo, before, after);
        }

        [Fact]
        public void CreateToken_IsNotExpiredImmediately()
        {
            var service = CreateService();
            var token = ParseToken(service.CreateToken(CreateUser()));

            Assert.True(token.ValidTo > DateTime.UtcNow);
        }

        // -------------------------------------------------------
        // Different roles produce correct claim value
        // -------------------------------------------------------
        [Theory]
        [InlineData(UserRole.SuperAdmin)]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.KeyUser)]
        [InlineData(UserRole.User)]
        public void CreateToken_AllRoles_ClaimMatchesRole(UserRole role)
        {
            var service = CreateService();
            var user = CreateUser();
            user.Role = role;

            var token = ParseToken(service.CreateToken(user));

            var claim = token.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role || c.Type == "role");

            Assert.NotNull(claim);
            Assert.Equal(role.ToString(), claim!.Value);
        }

        // -------------------------------------------------------
        // Missing configuration
        // -------------------------------------------------------
        [Fact]
        public void CreateToken_MissingJwtKey_Throws()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer",        Issuer    },
                    { "Jwt:Audience",      Audience  },
                    { "Jwt:ExpireMinutes", ExpiryMin }
                    // Key deliberately omitted
                })
                .Build();

            var service = new TokenService(config);

            Assert.ThrowsAny<Exception>(() => service.CreateToken(CreateUser()));
        }
    }
}