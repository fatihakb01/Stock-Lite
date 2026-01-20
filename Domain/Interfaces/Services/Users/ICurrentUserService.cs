namespace Domain.Interfaces.Services.Users;

public interface ICurrentUserService
{
    Guid Id { get; }
    Guid CompanyId { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
}
