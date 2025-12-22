using System.Security.Claims;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

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

    public Guid UserId =>
        Guid.Parse(User.FindFirst("nameid")!.Value);

    public Guid CompanyId =>
        Guid.Parse(User.FindFirst("companyId")!.Value);

    public bool IsAdmin =>
        User.IsInRole("Admin");

    public bool IsSuperAdmin =>
        User.IsInRole("SuperAdmin");
}
