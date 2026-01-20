using System.Security.Claims;
using Domain.Interfaces.Services.Users;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Users;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException();

    public Guid Id =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public Guid CompanyId =>
        Guid.Parse(User.FindFirst("companyId")!.Value);

    public bool IsAdmin =>
        User.IsInRole("Admin");

    public bool IsSuperAdmin =>
        User.IsInRole("SuperAdmin");
}
