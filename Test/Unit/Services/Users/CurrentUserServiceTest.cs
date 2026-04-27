using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Test.Unit.Services.Users;

public class CurrentUserServiceTest
{
    private static IHttpContextAccessor CreateContext(
        Guid userId,
        Guid companyId,
        bool isAdmin = false,
        bool isSuperAdmin = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("companyId", companyId.ToString())
        };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        if (isSuperAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(context);

        return accessor.Object;
    }

    [Fact]
    public void Id_ReturnsCorrectValue()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var service = new CurrentUserService(CreateContext(userId, companyId));

        Assert.Equal(userId, service.Id);
    }

    [Fact]
    public void CompanyId_ReturnsCorrectValue()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var service = new CurrentUserService(CreateContext(userId, companyId));

        Assert.Equal(companyId, service.CompanyId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAdmin_ReturnsTrue_WhenUserHasAdminRole(bool isAdmin)
    {
        var service = new CurrentUserService(
            CreateContext(Guid.NewGuid(), Guid.NewGuid(), isAdmin: isAdmin));

        Assert.True(service.IsAdmin);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsSuperAdmin_ReturnsTrue_WhenUserHasSuperAdminRole(bool isSuperAdmin)
    {
        var service = new CurrentUserService(
            CreateContext(Guid.NewGuid(), Guid.NewGuid(), isSuperAdmin: isSuperAdmin));

        Assert.True(service.IsSuperAdmin);
    }

    [Fact]
    public void Constructor_Throws_WhenNoHttpContext()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(accessor.Object);

        Assert.Throws<UnauthorizedAccessException>(() => service.Id);
    }
}